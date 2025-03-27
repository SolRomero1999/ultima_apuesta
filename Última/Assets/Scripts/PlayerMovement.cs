using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator animator;
    private Vector3 targetPosition;
    private bool isMoving = false;
    public bool canMove = true;
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
    }

    void Update()
    {
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

    bool IsPointerOverWall()
    {
        // Verificar si el clic fue sobre un objeto con tag "Wall"
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);
        
        return hit.collider != null && hit.collider.CompareTag("Wall");
    }

    void SetTargetPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        targetPosition = new Vector3(mousePosition.x, mousePosition.y, 0);
        isMoving = true;
    }

    void UpdateMovement()
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

    void MoveCharacter()
    {
        Vector3 direction = targetPosition - transform.position;
        direction.z = 0;

        if (direction.magnitude > 0.1f)
        {
            direction.Normalize();
            transform.position += direction * moveSpeed * Time.deltaTime;
            UpdateAnimation(direction);
            animator.SetBool("isIdle", false);
            transform.rotation = Quaternion.identity;
        }
        else
        {
            StopMovement();
        }
    }

    void UpdateAnimation(Vector3 direction)
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

    void OnCollisionEnter2D(Collision2D collision)
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

    void ForceIdleAnimation()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);
        transform.rotation = Quaternion.identity;
    }

    void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }
}