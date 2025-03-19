using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;  // Nombre de la escena a cargar (editable en el Inspector)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Verifica si el jugador entra en la zona
        {
            Debug.Log("Cambiando a la escena: " + sceneToLoad);
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
