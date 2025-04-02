using UnityEngine;

public class BartenderController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private int bartenderState;

    private void Start()
    {
        bartenderState = GameManager.Instance?.GetCurrentJudgeLevel() ?? 0;
        UpdateBartenderState();
    }

    private void UpdateBartenderState()
    {
        if (animator != null)
        {
            animator.SetInteger("BartenderState", bartenderState);
        }
    }

    public void SetInitialState()
    {
        bartenderState = 0;
        UpdateBartenderState();
    }

    public void SetAlternateState()
    {
        bartenderState = GameManager.Instance?.GetCurrentJudgeLevel() ?? 1;
        UpdateBartenderState();
    }

    public int GetBartenderState() => bartenderState;
}