using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float smoothSpeed = 5f;
    private Vector3 offset;

    private void Start()
    {
        if (player != null)
        {
            offset = transform.position - player.position;
        }
        else
        {
            Debug.LogWarning("Player reference is missing in CameraFollow");
        }
    }

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
        }
    }
}