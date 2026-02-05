using UnityEngine;

/// <summary>
/// 에임 커서 적용 및 씬 전환 유지
/// </summary>
public class CursorMgr : MonoBehaviour
{
    public Texture2D aimCursor;

    public static CursorMgr Inst;

    private void Awake()
    {
        if (Inst == null)
        {
            Inst = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ApplyAimCursor();
    }

    void ApplyAimCursor()
    {
        if (!aimCursor) return;

        // 커서 중심을 핫스팟으로 설정
        Vector2 hotSpot = new Vector2(aimCursor.width * 0.5f, aimCursor.height * 0.5f);

        Cursor.SetCursor(aimCursor, hotSpot, CursorMode.Auto);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
