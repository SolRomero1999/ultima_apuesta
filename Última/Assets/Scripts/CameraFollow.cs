using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // Referencia al jugador
    public float smoothSpeed = 5f; // Velocidad de suavizado
    private Vector3 offset; // Desplazamiento fijo de la cámara

    void Start()
    {
        offset = transform.position - player.position; // Guarda la distancia inicial
    }

    void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}
