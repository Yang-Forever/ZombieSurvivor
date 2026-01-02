using UnityEngine;
using UnityEngine.UI;

public enum PlayerState
{
    None,
    Play,
    LevelUp,
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

    public PlayerState state = PlayerState.None;

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
                configPanel.SetActive(true);
                Time.timeScale = 0;
            });

        if (configCloseBtn != null)
            configCloseBtn.onClick.AddListener(() =>
            {
                configPanel.SetActive(false);
                Time.timeScale = 1;
            });

        if (inven_Btn != null)
            inven_Btn.onClick.AddListener(() =>
            {
                invenPanel.SetActive(true);
                Time.timeScale = 0;
            });

        if (invenCloseBtn != null)
            invenCloseBtn.onClick.AddListener(() =>
            {
                invenPanel.SetActive(false);
                Time.timeScale = 1;
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
        curBullet_Text.text = Gun.Inst.curMagazine + " / ¡Ä";
    }

    public void GameStart()
    {
        ItemRuntimeData weapon = LevelUpMgr.Inst.FindRuntimeWeapon(ItemType.MainWeapon, "Pistol");

        Gun.Inst.SetWeapon(weapon);
    }
}
