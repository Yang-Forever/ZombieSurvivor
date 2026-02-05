using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 사운드 설정
/// 슬라이더(볼륨) 와 토글(ON/OFF)를 통해 사운드 옵션을 제어
/// PlayerPrefs에 설정 값을 저장/불러온다
/// </summary>
public class SoundUIMgr : MonoBehaviour
{
    public Slider soundSlider;
    public Toggle soundToggle;

    void Start()
    {
        // 저장된 값으로 UI 초기화
        soundSlider.value = PlayerPrefs.GetFloat("SoundVolume", 1f);
        soundToggle.isOn = PlayerPrefs.GetInt("SoundOnOff", 1) == 1;

        // UI 이벤트 연결
        soundSlider.onValueChanged.AddListener(OnVolumeChanged);
        soundToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    // 볼륨 슬라이더 변경 시 호출
    void OnVolumeChanged(float value)
    {
        Sound_Mgr.Inst.SoundVolume(value);
        PlayerPrefs.SetFloat("SoundVolume", value);
    }

    // 사운드 ON / OFF 토글 변경 시 호출
    void OnToggleChanged(bool isOn)
    {
        Sound_Mgr.Inst.SoundOnOff(isOn);
        PlayerPrefs.SetInt("SoundOnOff", isOn ? 1 : 0);
    }
}
