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
        private int _seSourceIndex;

        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;
        private float _seVolume = 1f;

        public float MasterVolume => _masterVolume;
        public float BGMVolume => _bgmVolume;
        public float SEVolume => _seVolume;

        private readonly Dictionary<string, AudioClip> _seCache = new();
        private readonly List<IDisposable> _subscriptions = new();

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
            foreach (var source in _seSources)
                source.loop = false;

            _ct = ct;

            _subscriptions.Add(EventBus.Subscribe<PlayBGMEvent>(e => PlayBGMAsync(e.Address, _ct).Forget()));
            _subscriptions.Add(EventBus.Subscribe<StopBGMEvent>(_ => StopBGM()));
            _subscriptions.Add(EventBus.Subscribe<PlaySEEvent>(e => PlaySEAsync(e.Address, _ct).Forget()));
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
        /// 指定したアドレスの SE を非同期でロードして再生する。同時再生数が上限に達している場合は最も古い SE を停止して再生する
        /// </summary>
        /// <param name="address">アセットのアドレスキー</param>
        /// <param name="ct">キャンセルトークン</param>
        public async UniTask PlaySEAsync(string address, CancellationToken ct)
        {
            if (!_seCache.TryGetValue(address, out var clip))
            {
                clip = await AddressablesLoader.LoadAsync<AudioClip>(address, ct);
                if (clip == null) return;
                _seCache[address] = clip;
            }

            var source = _seSources[_seSourceIndex];
            source.Stop();
            source.clip = clip;
            source.volume = _masterVolume * _seVolume;
            source.Play();

            _seSourceIndex = (_seSourceIndex + 1) % _seSources.Length;
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
        /// マスター音量を設定する。BGM・SE すべての音量に影響する
        /// </summary>
        /// <param name="volume">音量（0.0 〜 1.0）</param>
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _masterVolume * _bgmVolume;
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
        /// SE の音量を設定する
        /// </summary>
        /// <param name="volume">音量（0.0 〜 1.0）</param>
        public void SetSEVolume(float volume)
        {
            _seVolume = Mathf.Clamp01(volume);
        }
    }
}
