using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 게임 상태
public enum GameState
{
    Tutorial,
    Play,
    LevelUp,
    Inventory,
    Story,
    Option,
    GameEnd
}

/// <summary>
/// 게임 전체 흐름 및 상태 관리
/// 시간, UI, 난이도, 보스, 결과 처리 등
/// </summary>
public class GameMgr : MonoBehaviour
{
    [Header("UI Setting")]
    public float playTime = 900.0f;
    public Text levelText;
    public Text timeText;
    public Text scoreText;
    public Text killText;
    int score = 0;
    int killScore = 0;
    float uiTimer = 0f;                 // UI 갱신 간격용 타이머

    [Header("GameStory")]
    public GameObject GameStartStory;
    public GameObject GameBossStory;
    public GameObject GameWinStory;
    public Button StartCloseBtn;
    public Button BossCloseBtn;
    public Button WinCloseBtn;
    bool isBossemergence = false;       // 보스 첫 등장 여부 체크

    [Header("Inven Setting")]
    public Button inven_Btn;
    public Button invenCloseBtn;
    public GameObject invenPanel;

    [Header("Config Setting")]
    public Button config_Btn;
    public GameObject configPanel;
    public Button configCloseBtn;
    public Button ExitBtn;
    public Button tuto_Btn;
    public GameObject configTutoPanel;
    public Button configTutoExit_Btn;

    [Header("Difficulty")]
    public int difficultyLevel = 0;
    float difficultyInterval = 60f;     // 난이도 증가 주기
    float nextDifficultyTime = 840f;    // 다음 난이도 증가 시점

    [Header("Boss")]
    int bossLevel = 0;
    float bossInterval = 240f;          // 보스 등장 주기
    float nextBossTime = 660f;          // 다음 보스 등장 시점

    [Header("Result Setting")]
    public GameObject resultPanel;
    public Text infoText;
    public Text bestScoreText;
    public Text updateScoreText;
    public Button restart_Btn;
    public Button goTitle_Btn;

    [Header("Tutorial Setting")]
    public GameObject tutorialPanel;
    public Button tutoExit_Btn;

    [Header("Supply Box")]
    public GameObject boxPrefab;
    public BoxCollider mapBounds;       // 맵 범위
    float boxTimer = 30f;               // 박스 생성 주기

    public GameState state = GameState.Tutorial;

    public static GameMgr Inst;

    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (config_Btn != null)
            config_Btn.onClick.AddListener(() =>
            {
                if (state != GameState.Play)
                    return;

                PlayClick();

                configPanel.SetActive(true);
                ChangeState(GameState.Option);
            });

        if (configCloseBtn != null)
            configCloseBtn.onClick.AddListener(() =>
            {
                PlayClick();
                configPanel.SetActive(false);
                ChangeState(GameState.Play);
            });

        if (inven_Btn != null)
            inven_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                if (state != GameState.Play)
                    return;

                invenPanel.SetActive(true);
                ChangeState(GameState.Inventory);
            });

        if (invenCloseBtn != null)
            invenCloseBtn.onClick.AddListener(() =>
            {
                PlayClick();
                invenPanel.SetActive(false);
                ChangeState(GameState.Play);
            });

        if (ExitBtn != null)
            ExitBtn.onClick.AddListener(() =>
            {
                PlayClick();
                Time.timeScale = 1;
                FadeMgr.Inst.LoadScene("TitleScene");
            });

        if (restart_Btn != null)
            restart_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                Time.timeScale = 1;
                FadeMgr.Inst.LoadScene("GameScene");
            });

        if (goTitle_Btn != null)
            goTitle_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                Time.timeScale = 1;
                FadeMgr.Inst.LoadScene("TitleScene");
            });

        if (tutoExit_Btn != null)
            tutoExit_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                tutorialPanel.SetActive(false);
                ChangeState(GameState.Play);
            });

        if (tuto_Btn != null)
            tuto_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                configTutoPanel.SetActive(true);
                configPanel.SetActive(false);
            });

        if (configTutoExit_Btn != null)
            configTutoExit_Btn.onClick.AddListener(() =>
            {
                PlayClick();
                configTutoPanel.SetActive(false);
                configPanel.SetActive(true);
            });

        if (StartCloseBtn != null)
            StartCloseBtn.onClick.AddListener(() =>
            {
                PlayClick();
                GameStartStory.SetActive(false);
                tutorialPanel.SetActive(true);
                ChangeState(GameState.Tutorial);
            });

        if (BossCloseBtn != null)
            BossCloseBtn.onClick.AddListener(() =>
            {
                PlayClick();
                GameBossStory.SetActive(false);
                ChangeState(GameState.Play);

                nextBossTime -= bossInterval;
                bossLevel++;
                ZombieSpawner.Inst.SpawnBoss(bossLevel);
            });

        if (WinCloseBtn != null)
            WinCloseBtn.onClick.AddListener(() =>
            {
                PlayClick();
                GameWinStory.SetActive(false);
                GameWin();
            });

        ResetGame();
        GameStart();
    }

    void Update()
    {
        if (state != GameState.Play)
            return;

        playTime -= Time.deltaTime;

        // 최적화를 위한 UI 갱신
        uiTimer += Time.deltaTime;
        if (uiTimer >= 0.1f)
        {
            uiTimer = 0f;

            timeText.text = $"{(int)(playTime / 60):00} : {(int)(playTime % 60):00}";
            scoreText.text = "Score : " + score;
            killText.text = killScore.ToString();
        }

        boxTimer -= Time.deltaTime;

        if (boxTimer <= 0f)
        {
            SpawnBox();
            boxTimer = 30f;
        }

        CheckDifficulty();
        CheckBossSpawn();

        // 게임 승리
        if (playTime <= 0)
        {
            ChangeState(GameState.GameEnd);
            Sound_Mgr.Inst.StopBGM();
            GameWinStory.SetActive(true);
        }
    }

    // 게임 시작 시 기본무기 세팅
    public void GameStart()
    {
        ItemRuntimeData weapon = LevelUpMgr.Inst.FindRuntimeWeapon(MainWeaponType.Pistol);

        Gun.Inst.SetWeapon(weapon);
    }

    // 게임 상태 초기화
    void ResetGame()
    {
        playTime = 900f;
        score = 0;
        killScore = 0;

        difficultyLevel = 0;
        bossLevel = 0;
        isBossemergence = false;

        nextDifficultyTime = 840f;
        nextBossTime = 760f;

        PlayerStats.Inst.ResetStats();

        Zombie_Ctrl.NormalHpMul = 1f;
        Zombie_Ctrl.NormalSpeedMul = 1f;
        Zombie_Ctrl.NormalDmgMul = 1f;
        Zombie_Ctrl.BossHpMul = 1f;
        Zombie_Ctrl.BossSpeedMul = 1f;
        Zombie_Ctrl.BossDmgMul = 1f;

        ZombieSpawner.Inst.ResetSpawner();

        GameStartStory.SetActive(true);

        ChangeState(GameState.Story);
    }

    #region Game Result

    // 승리 처리
    public void GameWin()
    {
        Sound_Mgr.Inst.StopBGM();
        Sound_Mgr.Inst.PlayEffSound("GameWin", 0.8f);

        resultPanel.SetActive(true);

        int survivedTime = Mathf.RoundToInt(900f - playTime);
        int min = survivedTime / 60;
        int sec = survivedTime % 60;

        PlayerStats ps = PlayerStats.Inst;

        infoText.text =
            $"레벨 : {levelText.text}\n" +
            $"공격력 배율 : {ps.DamageMultiplier:0.0}\n" +
            $"공격속도 : {ps.AttackSpeed:0.00}\n" +
            $"이동속도 : {ps.MoveSpeed:0.0}\n" +
            $"자석범위 : {ps.MagnetRange:0.0}\n" +
            $"체력 : {ps.MaxHp:0}\n" +
            $"피해감소 : {(ps.DamageReduction * 100f):0}%\n" +
            $"관통 : {ps.Penetration}\n\n" +
            $"킬 : {killScore}\n" +
            $"생존시간 : {min:00}:{sec:00}";

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "최고기록\n" + bestScore + "\n\n\n" + "점수\n" + score;

        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            bestScoreText.text = "최고기록\n" + score + "\n\n\n" + "점수\n" + score;
            updateScoreText.text = "최고기록 갱신!";
        }
        else
        {
            updateScoreText.text = "";
        }

    }

    // 패배 처리
    public void GameEnd()
    {
        ChangeState(GameState.GameEnd);

        Sound_Mgr.Inst.StopBGM();
        Sound_Mgr.Inst.PlayEffSound("GameEnd", 0.8f);

        resultPanel.SetActive(true);

        int survivedTime = Mathf.RoundToInt(900f - playTime);
        int min = survivedTime / 60;
        int sec = survivedTime % 60;

        PlayerStats ps = PlayerStats.Inst;

        infoText.text =
            $"레벨 : {levelText.text}\n" +
            $"공격력 배율 : {ps.DamageMultiplier:0.0}\n" +
            $"공격속도 : {ps.AttackSpeed:0.00}\n" +
            $"이동속도 : {ps.MoveSpeed:0.0}\n" +
            $"자석범위 : {ps.MagnetRange:0.0}\n" +
            $"체력 : {ps.MaxHp:0}\n" +
            $"피해감소 : {(ps.DamageReduction * 100f):0}%\n" +
            $"관통 : {ps.Penetration}\n\n" +
            $"킬 : {killScore}\n" +
            $"생존시간 : {min:00}:{sec:00}";

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = "최고기록\n" + bestScore + "\n\n\n" + "점수\n" + score;

        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            bestScoreText.text = "최고기록\n" + score + "\n\n\n" + "점수\n" + score;
            updateScoreText.text = "최고기록 갱신!";
        }
        else
        {
            updateScoreText.text = "";
        }
    }
    #endregion

    // 플레이 상태 변경
    public void ChangeState(GameState newState)
    {
        state = newState;

        switch (state)
        {
            case GameState.Play:
                Time.timeScale = 1f;
                break;

            default:
                Time.timeScale = 0f;
                break;
        }
    }

    // 플레이 타임 기준 난이도 증가
    void CheckDifficulty()
    {
        if (playTime <= nextDifficultyTime)
        {
            difficultyLevel++;
            nextDifficultyTime -= difficultyInterval;
            ZombieSpawner.Inst.IncreaseDifficulty(difficultyLevel);
        }

    }

    // 플레이 타임 기준 보스 생성
    void CheckBossSpawn()
    {
        if (!isBossemergence && playTime <= nextBossTime)
        {
            isBossemergence = true;
            GameBossStory.SetActive(true);
            ChangeState(GameState.Story);
            return;
        }

        if (playTime <= nextBossTime)
        {
            nextBossTime -= bossInterval;
            bossLevel++;
            ZombieSpawner.Inst.SpawnBoss(bossLevel);
        }
    }

    // 좀비 처치 시 점수 휙득
    public void KillZombie(int value)
    {
        score += value + difficultyLevel * 10;
        killScore++;
    }

    // 맵 범위 내 랜덤 위치 박스 생성
    void SpawnBox()
    {
        Bounds b = mapBounds.bounds;

        float x = Random.Range(b.min.x + 1.5f, b.max.x - 1.5f);
        float z = Random.Range(b.min.z + 1.5f, b.max.z - 1.5f);

        Vector3 pos = new Vector3(x, 0f, z);
        Instantiate(boxPrefab, pos, Quaternion.identity);
    }
    
    // UI 클릭 사운드
    void PlayClick()
    {
        Sound_Mgr.Inst.PlayGUISound("UI_Click", 0.4f);
    }

    // UI 위에 마우스(클릭)이 있는지 판별
    public static bool IsPointerOverUIObject()
    {
        PointerEventData a_EDCurPos = new PointerEventData(EventSystem.current);

#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)

			List<RaycastResult> results = new List<RaycastResult>();
			for (int i = 0; i < Input.touchCount; ++i)
			{
				a_EDCurPos.position = Input.GetTouch(i).position;  
				results.Clear();
				EventSystem.current.RaycastAll(a_EDCurPos, results);
                if (0 < results.Count)
                    return true;
			}

			return false;
#else
        a_EDCurPos.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(a_EDCurPos, results);
        return (0 < results.Count);
#endif
    }//public bool IsPointerOverUIObject() 
}
