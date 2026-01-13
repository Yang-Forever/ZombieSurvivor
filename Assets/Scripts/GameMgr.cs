using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum PlayerState
{
    Tutorial,
    Play,
    LevelUp,
    Inventory,
    Option,
    GameEnd
}

public class GameMgr : MonoBehaviour
{
    [Header("UI Setting")]
    [HideInInspector] public float playTime = 900.0f;
    public Text levelText;
    public Text timeText;
    public Text scoreText;
    public Text killText;
    int score = 0;
    int killScore = 0;

    [Header("Inven Setting")]
    public Button inven_Btn;
    public Button invenCloseBtn;
    public GameObject invenPanel;

    [Header("Config Setting")]
    public Button config_Btn;
    public GameObject configPanel;
    public Button configCloseBtn;
    public Button ExitBtn;

    [Header("Difficulty")]
    public int difficultyLevel = 0;
    float difficultyInterval = 60f;
    float nextDifficultyTime = 840f;

    [Header("Boss")]
    float bossInterval = 240f;
    float nextBossTime = 760f;

    [Header("Result Setting")]
    public GameObject resultPanel;
    public Text infoText;
    public Text bestScoreText;
    public Text updateScoreText;
    public Button restart_Btn;
    public Button goLobby_Btn;

    [Header("Tutorial Setting")]
    public GameObject tutorialPanel;
    public Button tutoExit_Btn;

    public PlayerState state = PlayerState.Tutorial;

    public static GameMgr Inst;

    private void Awake()
    {
        Inst = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (config_Btn != null)
            config_Btn.onClick.AddListener(() =>
            {
                if (state != PlayerState.Play)
                    return;

                configPanel.SetActive(true);
                ChangeState(PlayerState.Option);
            });

        if (configCloseBtn != null)
            configCloseBtn.onClick.AddListener(() =>
            {
                configPanel.SetActive(false);
                ChangeState(PlayerState.Play);
            });

        if (inven_Btn != null)
            inven_Btn.onClick.AddListener(() =>
            {
                if (state != PlayerState.Play)
                    return;

                invenPanel.SetActive(true);
                ChangeState(PlayerState.Inventory);
            });

        if (invenCloseBtn != null)
            invenCloseBtn.onClick.AddListener(() =>
            {
                invenPanel.SetActive(false);
                ChangeState(PlayerState.Play);
            });

        if (ExitBtn != null)
            ExitBtn.onClick.AddListener(() =>
            {
                Application.Quit();
            });

        if (restart_Btn != null)
            restart_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("GameScene");
            });

        if (goLobby_Btn != null)
            goLobby_Btn.onClick.AddListener(() =>
            {
                SceneManager.LoadScene("LobbyScene");
            });

        if (tutoExit_Btn != null)
            tutoExit_Btn.onClick.AddListener(() =>
            {
                tutorialPanel.SetActive(false);
                ChangeState(PlayerState.Play);
            });

        ResetGame();
        GameStart();
    }

    void Update()
    {
        if (state != PlayerState.Play)
            return;

        playTime -= Time.deltaTime;

        timeText.text = $"{(int)(playTime / 60):00} : {(int)(playTime % 60):00}";
        scoreText.text = "Score : " + score;
        killText.text = killScore.ToString();

        CheckDifficulty();
        CheckBossSpawn();

        if (playTime <= 0)
        {
            state = PlayerState.GameEnd;
        }
    }

    public void GameStart()
    {
        ItemRuntimeData weapon = LevelUpMgr.Inst.FindRuntimeWeapon(MainWeaponType.Pistol);

        Gun.Inst.SetWeapon(weapon);
    }

    public void GameEnd()
    {
        ChangeState(PlayerState.GameEnd);

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
            $"점수 : {score}\n" +
            $"킬 : {killScore}\n" +
            $"생존시간 : {min:00}:{sec:00}";

        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = bestScore.ToString();

        if (score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", score);
            bestScoreText.text = "최고기록\n" + score;
            updateScoreText.text = "최고기록 갱신!";
        }
        else
        {
            updateScoreText.text = "";
        }
    }

    public void ChangeState(PlayerState newState)
    {
        state = newState;

        switch (state)
        {
            case PlayerState.Play:
                Time.timeScale = 1f;
                break;

            case PlayerState.Tutorial:
            case PlayerState.LevelUp:
            case PlayerState.Inventory:
            case PlayerState.Option:
            case PlayerState.GameEnd:
                Time.timeScale = 0f;
                break;
        }
    }

    void CheckDifficulty()
    {
        if (playTime <= nextDifficultyTime)
        {
            difficultyLevel++;
            nextDifficultyTime -= difficultyInterval;

            ZombieSpawner.Inst.IncreaseDifficulty(difficultyLevel);
        }
    }

    void CheckBossSpawn()
    {
        if (playTime <= nextBossTime)
        {
            nextBossTime -= bossInterval;
            ZombieSpawner.Inst.SpawnBoss();
        }
    }

    public void KillZombie(int value)
    {
        score += value + difficultyLevel * 10;
        killScore++;
    }

    void ResetGame()
    {
        playTime = 900f;
        score = 0;
        killScore = 0;

        difficultyLevel = 0;
        nextDifficultyTime = 840f;
        nextBossTime = 760f;

        PlayerStats.Inst.ResetStats();

        Zombie_Ctrl.HpMultiplier = 1f;
        Zombie_Ctrl.SpeedMultiplier = 1f;
        Zombie_Ctrl.DamageMultiplier = 1f;

        ZombieSpawner.Inst.ResetSpawner();

        tutorialPanel.SetActive(true);

        ChangeState(PlayerState.Tutorial);
    }

    public static bool IsPointerOverUIObject() //UGUI의 UI들이 먼저 피킹되는지 확인하는 함수
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
