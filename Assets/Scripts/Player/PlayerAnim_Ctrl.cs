using UnityEngine;

/// <summary>
/// 플레이어 애니메이션 파라미터를 제어하는 클래스
/// 이동 및 사망 애니메이션을 Animator에 전달
/// </summary>
public class PlayerAnim_Ctrl : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // 이동 방향 및 속도 애니메이션 갱신
    public void MoveAnim(float x, float z)
    {
        animator.SetFloat("MoveX", x);
        animator.SetFloat("MoveZ", z);

        float speed = new Vector2(x, z).magnitude;
        animator.SetFloat("Speed", speed);
    }

    // 사망 애니메이션 실행
    public void DieAnim()
    {
        animator.SetTrigger("Die");
    }
}
