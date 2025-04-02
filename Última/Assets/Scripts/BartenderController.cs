using UnityEngine;
public class BartenderController : MonoBehaviour
{
    [SerializeField] private Animator animator;  
    private int bartenderState = 0;  
    private void Start()
    {
        if (GameManager.instance != null)
        {
            bartenderState = GameManager.instance.GetCurrentJudgeLevel();
            
            if (bartenderState > 0)
            {
                SetAlternateState();
            }
            else
            {
                SetInitialState();
            }
        }
        else
        {
            SetInitialState();
        }
    }

    public void SetInitialState()
    {
        bartenderState = 0;

        if (animator != null)
        {
            animator.SetInteger("BartenderState", 0);
        }
    }

    public void SetAlternateState()
    {
        bartenderState = GameManager.instance != null ? GameManager.instance.GetCurrentJudgeLevel() : 1;

        if (animator != null)
        {
            animator.SetInteger("BartenderState", bartenderState);
        }
    }

    public int GetBartenderState()
    {
        return bartenderState;
    }
}