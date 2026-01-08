using UnityEngine;

public class PlayerAnim_Ctrl : MonoBehaviour
{
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void MoveAnim(float h, float v)
    {
        ResetTrigger();

        if (v > 0.1f)
            animator.SetTrigger("Move_F");
        else if (v < -0.1f)
            animator.SetTrigger("Move_B");
        else if (h > 0.1f)
            animator.SetTrigger("Move_R");
        else if (h < -0.1f)
            animator.SetTrigger("Move_L");
        else
            animator.SetTrigger("Stop");
    }

    void ResetTrigger()
    {
        animator.ResetTrigger("Move_F");
        animator.ResetTrigger("Move_B");
        animator.ResetTrigger("Move_L");
        animator.ResetTrigger("Move_R");
        animator.ResetTrigger("Stop");
    }

    public void ReloadAnim()
    {
        animator.SetTrigger("Reload");
    }

    public void DieAnim()
    {
        animator.SetTrigger("Die");
    }

}
