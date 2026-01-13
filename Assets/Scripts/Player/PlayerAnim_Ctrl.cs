using UnityEngine;

public class PlayerAnim_Ctrl : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void MoveAnim(float x, float z)
    {
        animator.SetFloat("MoveX", x);
        animator.SetFloat("MoveZ", z);

        float speed = new Vector2(x, z).magnitude;
        animator.SetFloat("Speed", speed);
    }

    public void DieAnim()
    {
        animator.SetTrigger("Die");
    }
}
