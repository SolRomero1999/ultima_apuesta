using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] public bool canMove = true;
    
    private Animator animator;
    private Rigidbody2D rb;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private int playerState = 0;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeComponents();
        SetInitialState();
    }

    private void Update()
    {
        if (!canMove)
        {
            ForceIdleAnimation();
            return;
        }

        HandleInput();
        UpdateMovement();
    }

    private void LateUpdate()
    {
        MaintainRotation();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision);
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void SetInitialState()
    {
        playerState = GameManager.Instance.GetPlayerState();
        animator.SetInteger("PlayerState", playerState);
    }
    #endregion

    #region Movement Logic
    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !IsPointerOverWall())
        {
            SetTargetPosition();
        }
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
    #endregion

    #region Animation Control
    private void UpdateAnimation(Vector3 direction)
    {
        // Reset all walking animations
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);

        // Determine primary direction
        if (Mathf.Abs(direction.x) * 0.7f > Mathf.Abs(direction.y))
        {
            animator.SetBool(direction.x > 0 ? "isWalkingRight" : "isWalkingLeft", true);
        }
        else
        {
            animator.SetBool(direction.y > 0 ? "isWalkingUp" : "isWalkingDown", true);
        }
    }

    private void ForceIdleAnimation()
    {
        animator.SetBool("isIdle", true);
        animator.SetBool("isWalkingUp", false);
        animator.SetBool("isWalkingDown", false);
        animator.SetBool("isWalkingLeft", false);
        animator.SetBool("isWalkingRight", false);
    }
    #endregion

    #region Utility Methods
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

    private void MaintainRotation()
    {
        transform.rotation = Quaternion.identity;
    }

    private void HandleCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Limites"))
        {
            StopMovement();
        }
    }
    #endregion

    #region Public Methods
    public void StopMovement()
    {
        isMoving = false;
        ForceIdleAnimation();
    }

    public void SetNextState()
    {
        int newState = (GameManager.Instance.GetPlayerState() + 1) % 3;
        GameManager.Instance.SetPlayerState(newState);
        Debug.Log("Nuevo playerState: " + newState);
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
        if (!enabled) StopMovement();
    }
    #endregion
}