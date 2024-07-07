using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AudioManager : SingletonMonoBehaviour<AudioManager>
{
    // Volume
    private const string BGM_VOLUME_KEY = "BGM_VOLUME_KEY";
    private const string SE_VOLUME_KEY  = "SE_VOLUME_KEY";
    private const float BGM_VOLUME_DEFAULT = 1.0f;
    private const float SE_VOLUME_DEFAULT = 1.0f;
    // Addressables のパス
    private const string BGM_PATH = "Audio/BGM/";
    private const string SE_PATH = "Audio/SE/";
    // AudioSource
    public AudioSource _bgmSource;
    public List<AudioSource> _seSourceList;
    private const int SE_SOURCE_NUM = 10;
    // 次に流すBGM
    public AudioClip _nextBgm = null;
    // BGM のフェードアウト設定
    public const float BGM_FADE_TIME_LONG = 0.9f;
    public const float BGM_FADE_TIME_SHORT  = 0.3f;
    private float _bgmFadeTime = BGM_FADE_TIME_SHORT;
    public bool _isFadeOut = false;

    /// <summary>
    /// 初期化
    /// </summary>    
    public override void Awake() {
        base.Awake();
        // AudioListener, AudioSource を作成
        gameObject.AddComponent<AudioListener>();
        for (int i = 0; i < SE_SOURCE_NUM + 1; i++) {
            gameObject.AddComponent<AudioSource>();
        }
        AudioSource[] audioSourceArray = GetComponents<AudioSource>();
        _seSourceList = new List<AudioSource>();
        for (int i = 0; i < audioSourceArray.Length; i++) {
            if (i == 0) {
                // BGM 用
                audioSourceArray[i].loop = true;
                _bgmSource = audioSourceArray[i];
                _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFAULT);
            } else {
                // SE 用
                _seSourceList.Add(audioSourceArray[i]);
                audioSourceArray[i].volume = PlayerPrefs.GetFloat(SE_VOLUME_KEY, SE_VOLUME_DEFAULT);
            }
        }
    }

    private void Update()
    {
        if (!_isFadeOut) return;

        // 徐々に音量を小さくしてフェードアウトさせる
        float baseVol = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFAULT);
        _bgmSource.volume -= baseVol / _bgmFadeTime * Time.deltaTime;
        if (_bgmSource.volume <= 0) {
            _bgmSource.Stop();
            _bgmSource.volume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, BGM_VOLUME_DEFAULT);
            _isFadeOut = false;
            // BGM切り替えの場合は次のBGMを流す
            if (_nextBgm != null) {
                _bgmSource.clip = _nextBgm;
                _bgmSource.Play();
                _nextBgm = null;
            }
        }
    }

    //=================================================================================
    // BGM
    //=================================================================================
    /// <summary>
    /// 任意のBGMを流す
    /// </summary>
    /// <param name="bgmName">ファイル名</param>
    /// <param name="fadeTime">フェードアウトにかかる時間</param>
    public async void PlayBgm(string bgmName, float fadeTime = BGM_FADE_TIME_LONG)
    {
        AudioClip bgm = await AssetManager.Instance.getResource<AudioClip>(BGM_PATH + bgmName);
        
        //現在BGMが流れていない時はそのまま流す
        if (!_bgmSource.isPlaying) {
            _bgmSource.clip = bgm;
            _bgmSource.Play();
        }
        //違うBGMが流れている時は、流れているBGMをフェードアウトさせてから次を流す
        else if (_bgmSource.clip.name != bgm.name) {
            _nextBgm = bgm;
            if (!_isFadeOut) {
                StopBgm(fadeTime);
            }
        }
    }

    /// <summary>
    /// BGMを止める
    /// </summary>
    /// <param name="fadeSpeedRate">フェードアウトにかかる時間</param>
    public void StopBgm(float fadeTime = 0f)
    {
        if (fadeTime == 0f) {
            _bgmSource.Stop();
            return;
        }
        // フェードアウトを設定
        _bgmFadeTime = fadeTime;
        _isFadeOut = true;
    }

    /// <summary>
    /// BGMの音量設定
    /// </summary>
    /// <param name="volume">BGM volume</param>
    public void SetBgmVolume(float volume)
    {
        _bgmSource.volume = volume;
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, volume);
    }

    //=================================================================================
    // SE
    //=================================================================================
    /// <summary>
    /// 任意のSEを流す
    /// </summary>
    /// <param name="seName">SE名</param>
    public async void PlaySe(string seName)
    {
        AudioClip se = await AssetManager.Instance.getResource<AudioClip>(SE_PATH + seName);
        if (se == default(AudioClip)) return;
        // SEの再生
        foreach(AudioSource seSource in _seSourceList){
            if(!seSource.isPlaying){
                seSource.PlayOneShot(se);
                return;
            }
        }
    }

    /// <summary>
    /// SEの音量設定
    /// </summary>
    /// <param name="volume">SE volume</param>
    public void SetSeVolume(float volume)
    {
        foreach (AudioSource seSource in _seSourceList) {
            seSource.volume = volume;
        }
        PlayerPrefs.SetFloat(SE_VOLUME_KEY, volume);
    }
}
