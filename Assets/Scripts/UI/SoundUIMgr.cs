using UnityEngine;
using UnityEngine.UI;

public class SoundUIMgr : MonoBehaviour
{
    public Slider soundSlider;
    public Toggle soundToggle;

    void Start()
    {
        // 1. 저장된 값으로 UI 초기화
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        soundToggle.isOn = PlayerPrefs.GetInt("SoundOnOff", 1) == 1;

        // 2. UI 이벤트 연결
        soundSlider.onValueChanged.AddListener(OnVolumeChanged);
        soundToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    void OnVolumeChanged(float value)
    {
        Sound_Mgr.Inst.SoundVolume(value);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    void OnToggleChanged(bool isOn)
    {
        Sound_Mgr.Inst.SoundOnOff(isOn);
        PlayerPrefs.SetInt("SoundOnOff", isOn ? 1 : 0);
    }
}
