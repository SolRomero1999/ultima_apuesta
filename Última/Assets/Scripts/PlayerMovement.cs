using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator animator;
    private Vector3 targetPosition;
    private bool isMoving = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Detectar el clic izquierdo del mouse
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Aseguramos que el Z no cambie (2D)

            targetPosition = mousePosition;
            isMoving = true;  // Iniciar el movimiento
        }

        if (isMoving)
        {
            MoveCharacter(targetPosition);
        }
        else
        {
            // Cuando el personaje se detiene, activar "Idle"
            animator.SetBool("isIdle", true);
        }
    }

    void MoveCharacter(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.z = 0; // Asegura que solo se mueva en 2D

        if (direction.magnitude > 0.1f)  // Movimiento continuo si el personaje no ha llegado
        {
            // Normaliza la dirección para mantener la misma velocidad en todas las direcciones
            direction.Normalize();

            transform.position += direction * moveSpeed * Time.deltaTime;

            // Desactivar todos los booleans de caminar
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalkingLeft", false);
            animator.SetBool("isWalkingRight", false);

            // Detectar la dirección en X y Y para las animaciones:
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Movimiento Horizontal
                if (direction.x > 0) // Derecha
                    animator.SetBool("isWalkingRight", true);
                else // Izquierda
                    animator.SetBool("isWalkingLeft", true);
            }
            else
            {
                // Movimiento Vertical
                if (direction.y > 0) // Arriba
                    animator.SetBool("isWalkingUp", true);
                else // Abajo
                    animator.SetBool("isWalkingDown", true);
            }

            // Asegúrate de que la animación de "Idle" no se active mientras el personaje se mueve
            animator.SetBool("isIdle", false);
        }
        else
        {
            // Cuando el personaje llega a la posición objetivo
            isMoving = false; // Detener el movimiento
            animator.SetBool("isIdle", true); // Activar "Idle"

            // Asegurarse de que los bools de caminar se desactiven
            animator.SetBool("isWalkingUp", false);
            animator.SetBool("isWalkingDown", false);
            animator.SetBool("isWalkingLeft", false);
            animator.SetBool("isWalkingRight", false);
        }
    }
}

