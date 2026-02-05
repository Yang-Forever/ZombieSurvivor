using UnityEngine;

/// <summary>
/// 플레이어를 따라다니는 탑다운 카메라
/// 맵 범위를 벗어나지 않도록 Clamp 처리
/// </summary>
public class FollowCam : MonoBehaviour
{
    [Header("Map Clamp")]
    public BoxCollider mapBounds;   // 맵 범위
    float camHalfHeight;
    float camHalfWidth;

    [Header("Follow Camera Setting")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 12f, -6f);
    public float followSpeed = 10f;


    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (!cam)
            cam = Camera.main;

        camHalfHeight = cam.orthographicSize;       // 카메라 세로 반 범위 = orthographic 카메라가 한쪽으로 보여주는 거리
        camHalfWidth = camHalfHeight * cam.aspect;  // 카메라 가로 반 범위 = 세로 반 범위 × 화면 비율(가로/ 세로)
    }

    void LateUpdate()
    {
        if (!target || !mapBounds)
            return;

        Vector3 desiredPos = target.position + offset;

        Bounds b = mapBounds.bounds;    // BoxCollider 범위

        // 카메라 크기만큼의 여유를 두고 Clamp
        float clampX = Mathf.Clamp(desiredPos.x, b.min.x + camHalfWidth, b.max.x - camHalfWidth);
        float clampZ = Mathf.Clamp(desiredPos.z, b.min.z + camHalfHeight, b.max.z - camHalfHeight);

        Vector3 finalPos = new Vector3(clampX, transform.position.y, clampZ);

        // 부드럽게 따라가도록 Lerp
        transform.position = Vector3.Lerp(transform.position, finalPos, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
