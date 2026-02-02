using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 12f, -6f);

    [Header("Map Clamp")]
    public BoxCollider mapBounds;   // ¸Ê³¡
    public float followSpeed = 10f;

    float camHalfHeight;
    float camHalfWidth;

    private void Start()
    {
        Camera cam = GetComponent<Camera>();
        if (!cam)
            cam = Camera.main;

        camHalfHeight = cam.orthographicSize;
        camHalfWidth = camHalfHeight * cam.aspect;
    }

    void LateUpdate()
    {
        if (!target || !mapBounds)
            return;

        Vector3 desiredPos = target.position + offset;

        Bounds b = mapBounds.bounds;

        float clampX = Mathf.Clamp(desiredPos.x, b.min.x + camHalfWidth, b.max.x - camHalfWidth);
        float clampZ = Mathf.Clamp(desiredPos.z, b.min.z + camHalfHeight, b.max.z - camHalfHeight);

        Vector3 finalPos = new Vector3(clampX, transform.position.y, clampZ);

        transform.position = Vector3.Lerp(transform.position, finalPos, followSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}
