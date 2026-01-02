using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 12f, -6f);

    void LateUpdate()
    {
        if (!target) return;

        transform.position = target.position + offset;
        // 탑뷰면 회전은 고정
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
