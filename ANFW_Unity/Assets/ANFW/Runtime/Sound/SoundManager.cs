using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ANFW.Sound
{
    public class SoundManager
    {
        private AudioSource _bgmSource;
        private AudioSource[] _seSources;
        private float[] _seSourcePlayTimes;

        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;
        private float _seVolume = 1f;

        public float MasterVolume => _masterVolume;
        public float BGMVolume => _bgmVolume;
        public float SEVolume => _seVolume;

        private readonly Dictionary<string, AudioClip> _seCache = new();
        private readonly List<IDisposable> _subscriptions = new();
        private readonly List<AudioSource> _positionalSources = new();

        private CancellationToken _ct;

        /// <summary>
        /// SoundManager を初期化して EventBus の購読を開始する
        /// </summary>
        /// <param name="bgmSource">BGM 再生用 AudioSource</param>
        /// <param name="seSources">SE 再生用 AudioSource 配列。配列の長さが同時再生の上限になる</param>
        /// <param name="ct">キャンセルトークン</param>
        public UniTask InitializeAsync(AudioSource bgmSource, AudioSource[] seSources, CancellationToken ct)
        {
            _bgmSource = bgmSource;
            _bgmSource.loop = true;

            _seSources = seSources;
            _seSourcePlayTimes = new float[seSources.Length];
            foreach (var source in _seSources)
                source.loop = false;

            _ct = ct;

            _subscriptions.Add(EventBus.Subscribe<PlayBGMEvent>(e => PlayBGMAsync(e.Address, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<StopBGMEvent>(_ => StopBGM()));
            _subscriptions.Add(EventBus.Subscribe<PlaySEEvent>(e => PlaySEAsync(e.Address, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<PlaySEAtPositionEvent>(e => PlaySEAtPositionAsync(e.Address, e.Position, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<SetMasterVolumeEvent>(e => SetMasterVolume(e.Volume)));
            _subscriptions.Add(EventBus.Subscribe<SetBGMVolumeEvent>(e => SetBGMVolume(e.Volume)));
            _subscriptions.Add(EventBus.Subscribe<SetSEVolumeEvent>(e => SetSEVolume(e.Volume)));

            ANFWLogger.Log("SoundManager: Initialized");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 購読をすべて解除して再生中の音声とキャッシュを解放する
        /// </summary>
        public void Dispose()
        {
            foreach (var subscription in _subscriptions)
                subscription.Dispose();
            _subscriptions.Clear();

            StopBGM();
            ReleaseAllSECache();
            _positionalSources.Clear();
        }

        /// <summary>
        /// 指定したアドレスの BGM を非同期でロードして再生する。再生中の BGM がある場合は停止して解放する
        /// </summary>
        /// <param name="address">アセットのアドレスキー</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PlayBGMAsync(string address, CancellationToken ct)
        {
            StopBGM();

            var clip = await AddressablesLoader.LoadAsync<AudioClip>(address, ct);
            if (clip == null) return;

            _bgmSource.clip = clip;
            _bgmSource.volume = _masterVolume * _bgmVolume;
            _bgmSource.Play();
        }

        /// <summary>
        /// BGM を停止してクリップを解放する
        /// </summary>
        public void StopBGM()
        {
            if (_bgmSource.clip == null) return;

            _bgmSource.Stop();
            AddressablesLoader.Release(_bgmSource.clip);
            _bgmSource.clip = null;
        }

        /// <summary>
        /// 指定したアドレスの SE を非同期でロードして 2D 再生する。同時再生数が上限に達している場合は最も古い SE を停止して再生する
        /// </summary>
        /// <param name="address">アセットのアドレスキー</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PlaySEAsync(string address, CancellationToken ct)
        {
            var clip = await GetOrLoadSEClipAsync(address, ct);
            if (clip == null) return;

            var source = GetNextSESource();
            source.spatialBlend = 0f;
            source.clip = clip;
            source.volume = _masterVolume * _seVolume;
            source.Play();
        }

        /// <summary>
        /// 指定したアドレスの SE を非同期でロードして、ワールド空間の指定位置から 3D 再生する
        /// </summary>
        /// <param name="address">アセットのアドレスキー</param>
        /// <param name="position">再生する位置（ワールド座標）</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PlaySEAtPositionAsync(string address, Vector3 position, CancellationToken ct)
        {
            var clip = await GetOrLoadSEClipAsync(address, ct);
            if (clip == null) return;

            var source = GetNextSESource();
            source.transform.position = position;
            source.spatialBlend = 1f;
            source.clip = clip;
            source.volume = _masterVolume * _seVolume;
            source.Play();
        }

        private async UniTask<AudioClip> GetOrLoadSEClipAsync(string address, CancellationToken ct)
        {
            if (!_seCache.TryGetValue(address, out var clip))
            {
                clip = await AddressablesLoader.LoadAsync<AudioClip>(address, ct);
                if (clip == null) return null;
                _seCache[address] = clip;
            }
            return clip;
        }

        private AudioSource GetNextSESource()
        {
            for (var i = 0; i < _seSources.Length; i++)
            {
                if (!_seSources[i].isPlaying)
                {
                    _seSourcePlayTimes[i] = Time.time;
                    return _seSources[i];
                }
            }

            // すべて再生中の場合は再生開始時刻が最も古いソースを停止して再利用する
            var oldestIndex = 0;
            for (var i = 1; i < _seSources.Length; i++)
            {
                if (_seSourcePlayTimes[i] < _seSourcePlayTimes[oldestIndex])
                    oldestIndex = i;
            }

            _seSources[oldestIndex].Stop();
            _seSourcePlayTimes[oldestIndex] = Time.time;
            return _seSources[oldestIndex];
        }

        /// <summary>
        /// キャッシュ済みの SE クリップをすべて解放する
        /// </summary>
        public void ReleaseAllSECache()
        {
            foreach (var clip in _seCache.Values)
                AddressablesLoader.Release(clip);
            _seCache.Clear();
        }

        /// <summary>
        /// マスター音量を設定する。BGM・SE・ポジショナルソースすべての音量に影響する
        /// </summary>
        /// <param name="volume">音量（0.0 〜 1.0）</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _masterVolume * _bgmVolume;
            SyncPositionalSourceVolumes();
        }

        /// <summary>
        /// BGM の音量を設定する
        /// </summary>
        /// <param name="volume">音量（0.0 〜 1.0）</param>
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _masterVolume * _bgmVolume;
        }

        /// <summary>
        /// SE の音量を設定する。ポジショナルソースにも反映される
        /// </summary>
        /// <param name="volume">音量（0.0 〜 1.0）</param>
        public void SetSEVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
            SyncPositionalSourceVolumes();
        }

        /// <summary>
        /// 指定した Transform の子として 3D 再生用の AudioSource を生成し SoundManager の管理下に置く。
        /// キャラクターなど動くオブジェクトの初期化時に一度だけ呼ぶ
        /// </summary>
        /// <param name="parent">AudioSource を子として追加する Transform</param>
        /// <returns>生成した AudioSource</returns>
        public AudioSource CreatePositionalSource(Transform parent)
        {
            var go = new GameObject("PositionalSESource");
            go.transform.SetParent(parent);
            go.transform.localPosition = Vector3.zero;

            var source = go.AddComponent<AudioSource>();
            source.spatialBlend = 1f;
            source.loop = false;
            source.volume = _masterVolume * _seVolume;

            _positionalSources.Add(source);

            ANFWLogger.Log($"SoundManager: Created positional source on '{parent.name}'");
            return source;
        }

        /// <summary>
        /// CreatePositionalSource で生成した AudioSource を管理対象から外す。GameObject の破棄はゲーム側で行う
        /// </summary>
        /// <param name="source">解放する AudioSource</param>
        public void ReleasePositionalSource(AudioSource source)
        {
            _positionalSources.Remove(source);
        }

        private void SyncPositionalSourceVolumes()
        {
            for (var i = _positionalSources.Count - 1; i >= 0; i--)
            {
                // 破棄済みオブジェクトを自動クリーンアップ
                if (_positionalSources[i] == null)
                {
                    _positionalSources.RemoveAt(i);
                    continue;
                }
                _positionalSources[i].volume = _masterVolume * _seVolume;
            }
        }
    }
}
