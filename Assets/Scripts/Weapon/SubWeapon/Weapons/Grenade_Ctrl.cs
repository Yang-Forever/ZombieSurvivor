using UnityEngine;

public class Grenade_Ctrl : MonoBehaviour
{
    float damage = 0;
    float radius = 0;

    private void OnTriggerEnter(Collider colll)
    {
        if(colll.CompareTag("Floor"))
        {
            // 범위 내에 데미지를 주는 함수
        }
    }
}
