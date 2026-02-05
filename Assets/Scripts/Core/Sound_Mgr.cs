using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 사운드 관리
/// BGM / 효과음 / 루프 효과음 처리
/// DontDestroy 기반 싱글톤
/// </summary>
public class Sound_Mgr : MonoBehaviour
{
    [HideInInspector] public AudioSource m_AudioSrc = null;
    Dictionary<string, AudioClip> m_AdClipList = new Dictionary<string, AudioClip>();

    float m_bgmVolume = 0.2f;
    [HideInInspector] public bool m_SoundOnOff = true;
    [HideInInspector] public float m_SoundVolume = 1.0f;

    int m_EffSdCount = 5;
    int m_SoundCount = 0;
    GameObject[] m_SndObjList = new GameObject[10];
    AudioSource[] m_SndSrcList = new AudioSource[10];
    float[] m_EffVolume = new float[10];

    AudioSource m_LoopEffSrc;
    string m_CurLoopSound = "";

    Dictionary<string, float> m_LastPlayTime = new Dictionary<string, float>();

    public static Sound_Mgr Inst;

    private void Awake()
    {
        if(Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);

            LoadChildGameObj();             // AudioSource 초기 구성
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Resources/Sounds 폴더의 모든 AudioClip 로드
    void Start()
    {
        AudioClip a_GAudioClip = null;
        object[] temp = Resources.LoadAll("Sounds");
        for (int i = 0; i < temp.Length; i++)
        {
            a_GAudioClip = temp[i] as AudioClip;

            if (m_AdClipList.ContainsKey(a_GAudioClip.name) == true)
                continue;

            m_AdClipList.Add(a_GAudioClip.name, a_GAudioClip);
        }   
    }

    // AudioSource 및 하위 오브젝트 초기화
    void LoadChildGameObj()
    {
        m_AudioSrc = gameObject.AddComponent<AudioSource>();

        // 효과음 풀 생성
        for (int i = 0; i < m_EffSdCount; i++)
        {
            GameObject newSndObj = new GameObject();
            newSndObj.transform.SetParent(transform);
            newSndObj.transform.localPosition = Vector3.zero;
            AudioSource a_AudioSrc = newSndObj.AddComponent<AudioSource>();
            a_AudioSrc.playOnAwake = false;
            a_AudioSrc.loop = false;
            newSndObj.name = "SoundEffObj";

            m_SndSrcList[i] = a_AudioSrc;
            m_SndObjList[i] = newSndObj;
        }

        // 루프 효과음 AudioSoruce 생성
        m_LoopEffSrc = gameObject.AddComponent<AudioSource>();
        m_LoopEffSrc.playOnAwake = false;
        m_LoopEffSrc.loop = true;

        // 사운드 옵션 로드
        int a_SoundOnOff = PlayerPrefs.GetInt("SoundOnOff", 1);
        if (a_SoundOnOff == 1)
            SoundOnOff(true);
        else
            SoundOnOff(false);

        float a_Value = PlayerPrefs.GetFloat("SoundVolume", 1.0f);
        SoundVolume(a_Value);
    }

    // 배경 음악 재생
    public void PlayBGM(string a_FileName, float fVolume = 0.2f)
    {
        AudioClip a_GAudioClip = null;
        if (m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName];
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_AdClipList.Add(a_FileName, a_GAudioClip);
        }

        if (m_AudioSrc == null)
            return;

        if (m_AudioSrc.clip != null && m_AudioSrc.clip.name == a_FileName)
            return;

        m_AudioSrc.clip = a_GAudioClip;
        m_AudioSrc.volume = fVolume * m_SoundVolume;
        m_bgmVolume = fVolume;
        m_AudioSrc.loop = true;
        m_AudioSrc.Play();

    }

    // UI 버튼 클릭 등 짧은 효과음
    public void PlayGUISound(string a_FileName, float fVolume = 0.2f)
    {

        if (m_SoundOnOff == false)
            return;

        AudioClip a_GAudioClip = null;
        if (m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName];
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_AdClipList.Add(a_FileName, a_GAudioClip);
        }

        if (m_AudioSrc == null)
            return;

        m_AudioSrc.PlayOneShot(a_GAudioClip, fVolume * m_SoundVolume);

    }

    // 일반 효과음 재생
    public void PlayEffSound(string a_FileName, float fVolume = 0.2f)
    {
        if (m_SoundOnOff == false)
            return;

        AudioClip a_GAudioClip = null;
        if (m_AdClipList.ContainsKey(a_FileName) == true)
        {
            a_GAudioClip = m_AdClipList[a_FileName];
        }
        else
        {
            a_GAudioClip = Resources.Load("Sounds/" + a_FileName) as AudioClip;
            m_AdClipList.Add(a_FileName, a_GAudioClip);
        }

        if (a_GAudioClip == null)
            return;

        if (m_SndSrcList[m_SoundCount] != null)
        {
            m_SndSrcList[m_SoundCount].volume = 1.0f;
            m_SndSrcList[m_SoundCount].PlayOneShot(a_GAudioClip, fVolume * m_SoundVolume);
            m_EffVolume[m_SoundCount] = fVolume;

            m_SoundCount++;
            if (m_EffSdCount <= m_SoundCount)
                m_SoundCount = 0;
        }

    }

    // 효과음 겹침 방지
    public void PlayEffSoundLimit(string soundName, float volume = 1f, float cooldown = 0.05f)
    {
        if (m_SoundOnOff == false)
            return;

        float now = Time.time;

        if (m_LastPlayTime.ContainsKey(soundName))
        {
            if (now - m_LastPlayTime[soundName] < cooldown)
                return;
        }

        m_LastPlayTime[soundName] = now;

        PlayEffSound(soundName, volume);
    }

    // 루프 효과음
    public void PlayLoopEffect(string soundName, float volume = 1f)
    {
        if (!m_SoundOnOff)
            return;

        if (m_CurLoopSound == soundName && m_LoopEffSrc.isPlaying)
            return;

        AudioClip clip = null;
        if (m_AdClipList.ContainsKey(soundName))
            clip = m_AdClipList[soundName];
        else
        {
            clip = Resources.Load<AudioClip>("Sounds/" + soundName);
            m_AdClipList.Add(soundName, clip);
        }

        if (clip == null)
            return;

        m_CurLoopSound = soundName;
        m_LoopEffSrc.clip = clip;
        m_LoopEffSrc.volume = volume * m_SoundVolume;
        m_LoopEffSrc.Play();
    }

    // 루프 효과음 정지
    public void StopLoopEffect(string soundName)
    {
        if (m_LoopEffSrc == null)
            return;

        if (m_CurLoopSound != soundName)
            return;

        m_LoopEffSrc.Stop();
        m_LoopEffSrc.clip = null;
        m_CurLoopSound = "";
    }

    // 전체 사운드 On / Off
    public void SoundOnOff(bool a_OnOff = true)
    {
        bool a_MuteOnOff = !a_OnOff;

        if (m_AudioSrc != null)
        {
            m_AudioSrc.mute = a_MuteOnOff;
        }

        for (int i = 0; i < m_EffSdCount; i++)
        {
            if (m_SndSrcList[i] != null)
            {
                m_SndSrcList[i].mute = a_MuteOnOff;

                if (a_MuteOnOff == false)
                    m_SndSrcList[i].time = 0;
            }
        }

        m_SoundOnOff = a_OnOff;
    }

    // 전체 볼륨 조절
    public void SoundVolume(float fVolume)
    {
        if (m_AudioSrc != null)
            m_AudioSrc.volume = m_bgmVolume * fVolume;

        m_SoundVolume = fVolume;
    }

    public void StopBGM()
    {
        if (m_AudioSrc != null && m_AudioSrc.isPlaying)
            m_AudioSrc.Pause();
    }

    public void ResumeBGM()
    {
        if (m_AudioSrc != null && !m_AudioSrc.isPlaying)
            m_AudioSrc.UnPause();
    }
}
