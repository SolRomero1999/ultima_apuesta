using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    private bool isMoving = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        int playerState = GameManager.instance.GetPlayerState();
        animator.SetInteger("PlayerState", playerState);
    }

    private void Update()
    {
        int playerState = GameManager.instance.GetPlayerState(); 
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        isMoving = moveX != 0 || moveY != 0;

        if (isMoving)
        {
            if (moveY > 0) animator.Play("WalkUp_" + playerState);
            else if (moveY < 0) animator.Play("WalkDown_" + playerState);
            else if (moveX > 0) animator.Play("WalkRight_" + playerState);
            else if (moveX < 0) animator.Play("WalkLeft_" + playerState);
        }
        else
        {
            animator.Play("Idle_" + playerState);
        }
    }

    // Cambiar estado de animación, pero dejar que GameManager maneje la lógica
    public void SetNextState()
    {
        int newState = (GameManager.instance.GetPlayerState() + 1) % 3;
        GameManager.instance.SetPlayerState(newState); 
        animator.SetInteger("PlayerState", newState);
        Debug.Log("Nuevo playerState: " + newState);
    }
}