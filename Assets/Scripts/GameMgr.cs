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
    public Text curBullet_Text;
    public Text levelText;

    [Header("Inven Setting")]
    public Button inven_Btn;
    public Button invenCloseBtn;
    public GameObject invenPanel;

    [Header("Config Setting")]
    public Button config_Btn;
    public GameObject configPanel;
    public Button configCloseBtn;
    public Button ExitBtn;

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

    // Update is called once per frame
    void Update()
    {
        curBullet_Text.text = Gun.Inst.curMagazine + " / ∞";
    }

    public void GameStart()
    {
        ItemRuntimeData weapon = LevelUpMgr.Inst.FindRuntimeWeapon(ItemType.MainWeapon, "Pistol");

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
