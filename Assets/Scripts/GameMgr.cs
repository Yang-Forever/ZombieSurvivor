using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum PlayerState
{
    Play,
    LevelUp,
    Inventory,
    Option,
    Die
}

public class GameMgr : MonoBehaviour
{
    [Header("UI Setting")]
    public Text levelText;
    public Text timeText;
    public Text bossText;
    [HideInInspector] public float playTime = 0.0f;

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
    float difficultyInterval = 180f;
    float nextDifficultyTime = 180f;

    [Header("Boss")]
    float bossInterval = 300f;
    float nextBossTime = 300f;

    public PlayerState state = PlayerState.Play;

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

        state = PlayerState.Play;

        GameStart();
    }

    void Update()
    {
        if (state != PlayerState.Play)
            return;

        playTime += Time.deltaTime;

        timeText.text = $"{(int)(playTime / 60):00} : {(int)(playTime % 60):00}";

        CheckDifficulty();
        CheckBossSpawn();
    }

    public void GameStart()
    {
        ItemRuntimeData weapon = LevelUpMgr.Inst.FindRuntimeWeapon(MainWeaponType.Pistol);

        Gun.Inst.SetWeapon(weapon);
    }

    public void ChangeState(PlayerState newState)
    {
        state = newState;

        switch (state)
        {
            case PlayerState.Play:
                Time.timeScale = 1f;
                break;

            case PlayerState.LevelUp:
            case PlayerState.Inventory:
            case PlayerState.Option:
            case PlayerState.Die:
                Time.timeScale = 0f;
                break;
        }
    }

    void CheckDifficulty()
    {
        if (playTime >= nextDifficultyTime)
        {
            difficultyLevel++;
            nextDifficultyTime += difficultyInterval;

            ZombieSpawner.Inst.IncreaseDifficulty(difficultyLevel);
        }
    }

    void CheckBossSpawn()
    {
        if (playTime >= nextBossTime)
        {
            nextBossTime += bossInterval;
            ZombieSpawner.Inst.SpawnBoss();
        }
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
