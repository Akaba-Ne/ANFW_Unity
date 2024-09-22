using UnityEngine;
using UnityEngine.UI;

namespace Sample {
    public class SampleAudioEvent : MonoBehaviour
    {
        [SerializeField]
        private Slider _bgmVolSlider;
        [SerializeField]
        private Slider _seVolSlider;

        private void Awake()
        {
            _seVolSlider.value = PlayerPrefs.GetFloat("BGM_VOLUME_KEY", 1.0f);
            _bgmVolSlider.value = PlayerPrefs.GetFloat("SE_VOLUME_KEY", 1.0f);
        }

        public void PressButtonPlaySe01()
        {
            AudioManager.Instance.PlaySe("button_1");
        }

        public void PressButtonPlayBgm01()
        {
            AudioManager.Instance.PlayBgm("Test/01");
        }

        public void PressButtonPlayBgm02()
        {
            AudioManager.Instance.PlayBgm("Test/02");
        }

        public void PressButtonStopBgm()
        {
            AudioManager.Instance.StopBgm();
        }

        public void ChangedSliderBgmVolume()
        {
            AudioManager.Instance.SetBgmVolume(_bgmVolSlider.value);
            PlayerPrefs.SetFloat("BGM_VOLUME_KEY", _bgmVolSlider.value);
        }

        public void ChangedSliderSeVolume()
        {
            AudioManager.Instance.SetSeVolume(_seVolSlider.value);
            PlayerPrefs.SetFloat("SE_VOLUME_KEY", _seVolSlider.value);
        }
    }
}
