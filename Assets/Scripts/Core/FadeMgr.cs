using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환시 페이드 인/아웃 + 로딩 + BGM 전환 담당
/// DontDestroyOnLoad을 사용하여 씬 전환 시에도 유지
/// </summary>
public class FadeMgr : MonoBehaviour
{
    [Header("UI Settting")]
    public Image fadeImg;           // 페이드(암막)용 이미지
    public GameObject loading;      // 로딩 이미지
    public Image blockPanel;        // 입력 차단 용

    public float fadeTime = 0.5f;   // 페이드 인/아웃에 걸리는 시간
    public float loadingTime = 2f;  // 최소 로딩 시간
    bool isTransition = false;      // 중복 입력 방지

    public static FadeMgr Inst;     // 싱글톤 인스턴스

    void Awake()
    {
        if(Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);  // 씬 전환시에도 유지
        }
        else
        {
            Destroy(gameObject);    // 중복 시 제거
            return;
        }
    }

    // 외부에서 씬 전환에 사용하는 함수
    public void LoadScene(string sceneName)
    {
        if (isTransition)
            return;

        StartCoroutine(FadeInOut(sceneName));
    }

    // 페이드 아웃 -> 씬 로딩 -> 페이드 인의 흐름을 가진 코루틴
    IEnumerator FadeInOut(string sceneName)
    {
        isTransition = true;
        blockPanel.gameObject.SetActive(true);

        // 페이드 아웃
        yield return Fade(0f, 1f);

        Sound_Mgr.Inst.StopBGM();

        loading.SetActive(true);

        // 비동기 씬 로딩
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false; // 로딩 완료 후 수동으로 씬 활성화

        float timer = 0f;

        // 로딩 진행률이 90% 이상, 최소 로딩 시간 충족까지 대기
        while (op.progress < 0.9f || timer < loadingTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 씬 활성화
        op.allowSceneActivation = true;
        yield return null;

        loading.SetActive(false);

        // 페이드 인
        yield return Fade(1f, 0f);

        PlaySceneBGM(sceneName);

        blockPanel.gameObject.SetActive(false);
        isTransition = false;
    }

    // FadeImg의 알파값을 이용한 페이드 처리
    IEnumerator Fade(float from, float to)
    {
        float t = 0f;
        Color c = fadeImg.color;

        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Lerp(from, to, t / fadeTime);
            fadeImg.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        fadeImg.color = new Color(c.r, c.g, c.b, to);
    }

    // 화면 전환 시 사용되는 BGM
    void PlaySceneBGM(string sceneName)
    {
        switch (sceneName)
        {
            case "TitleScene":
                Sound_Mgr.Inst.PlayBGM("BGM_Title", 0.2f);
                break;

            case "GameScene":
                Sound_Mgr.Inst.PlayBGM("BGM_Game", 0.2f);
                break;
        }
    }

}
