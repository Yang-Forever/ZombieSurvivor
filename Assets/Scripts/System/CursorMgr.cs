using UnityEngine;

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

        Vector2 hotSpot = new Vector2(aimCursor.width * 0.5f, aimCursor.height * 0.5f);

        Cursor.SetCursor(aimCursor, hotSpot, CursorMode.Auto);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
