using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeMgr : MonoBehaviour
{
    [Header("UI Settting")]
    public Image fadeImg;
    public GameObject loading;
    public Image blockPanel;

    public float fadeTime = 0.5f;
    public float loadingTime = 2f;
    bool isTransition = false;  // 중복 방지

    public static FadeMgr Inst;

    void Awake()
    {
        if(Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void LoadScene(string sceneName)
    {
        if (isTransition)
            return;

        StartCoroutine(FadeInOut(sceneName));
    }

    IEnumerator FadeInOut(string sceneName)
    {
        isTransition = true;
        blockPanel.gameObject.SetActive(true);

        yield return Fade(0f, 1f);

        Sound_Mgr.Inst.StopBGM();

        loading.SetActive(true);
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        float timer = 0f;

        while (op.progress < 0.9f || timer < loadingTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        op.allowSceneActivation = true;

        yield return null;

        loading.SetActive(false);

        yield return Fade(1f, 0f);

        PlaySceneBGM(sceneName);

        blockPanel.gameObject.SetActive(false);
        isTransition = false;
    }

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
