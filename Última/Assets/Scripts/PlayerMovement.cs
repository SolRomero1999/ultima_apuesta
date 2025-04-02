using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public bool canMove = true;
    private int playerState = 0;
    private Animator animator;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private Rigidbody2D rb;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        
        playerState = GameManager.instance.GetPlayerState();
        animator.SetInteger("PlayerState", playerState);
    }

    void Update()
    {
        playerState = GameManager.instance.GetPlayerState();
        
        if (!canMove)
        {
            ForceIdleAnimation();
            return;
        }
        
        if (Input.GetMouseButtonDown(0) && !IsPointerOverWall())
        {
            SetTargetPosition();
        }

        UpdateMovement();
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }

    private bool IsPointerOverWall()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        return hit.collider != null && hit.collider.CompareTag("Wall");
    }

    private void SetTargetPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition = new Vector3(mousePosition.x, mousePosition.y, 0);
        isMoving = true;
    }

    private void UpdateMovement()
    {
        if (isMoving)
        {
            MoveCharacter();
        }
        else
        {
            animator.SetBool("isIdle", true);
        }
    }

    private void MoveCharacter()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0;

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;
            UpdateAnimation(direction);
            animator.SetBool("isIdle", false);
        }
        else
        {
            StopMovement();
        }
    }

    private void UpdateAnimation(Vector3 direction)
    {
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);

        if (Mathf.Abs(direction.x) * 0.7f > Mathf.Abs(direction.y))
        {
            animator.SetBool(direction.x > 0 ? "isWalkingRight" : "isWalkingLeft", true);
        }
        else
        {
            animator.SetBool(direction.y > 0 ? "isWalkingUp" : "isWalkingDown", true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Limites"))
        {
            StopMovement();
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        ForceIdleAnimation();
    }

    private void ForceIdleAnimation()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);
    }

    public void SetNextState()
    {
        int newState = (GameManager.instance.GetPlayerState() + 1) % 3;
        GameManager.instance.SetPlayerState(newState);
        Debug.Log("Nuevo playerState: " + newState);
    }
}
