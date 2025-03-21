using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;  // Nombre de la escena a cargar (editable en el Inspector)
    [SerializeField] private string dialogMessage; // Mensaje personalizado para el diálogo
    [SerializeField] private GameObject dialogPrefab; // Prefab del diálogo (con Panel, TMP_Text y Buttons)
    [SerializeField] private bool showOnlyMessage = false; // Mostrar solo un mensaje con un botón "Claro"
    [SerializeField] private Sprite dialogBackgroundImage; // Imagen de fondo para el diálogo (opcional)

    private GameObject dialogInstance; // Instancia del diálogo
    private PlayerMovement playerMovement; // Referencia al script de movimiento del jugador

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))  // Verifica si el jugador entra en la zona
        {
            playerMovement = other.GetComponent<PlayerMovement>(); // Obtén el componente PlayerMovement
            if (playerMovement != null)
            {
                playerMovement.StopMovement(); // Detener el movimiento del jugador
                playerMovement.canMove = false; // Deshabilita el movimiento del jugador
            }
            ShowDialog(); // Muestra el diálogo
        }
    }

    private void ShowDialog()
    {
        if (dialogPrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab de diálogo.");
            return;
        }

        // Instancia el diálogo en la escena
        dialogInstance = Instantiate(dialogPrefab, Vector3.zero, Quaternion.identity);

        // Asegúrate de que el diálogo esté en la capa de la interfaz de usuario (UI)
        dialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);

        // Activa el diálogo
        dialogInstance.SetActive(true);

        // Obtén las referencias a los componentes del diálogo
        TMP_Text messageText = dialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText == null)
        {
            Debug.LogError("No se encontró un componente TMP_Text en el diálogo.");
            return;
        }

        // Asigna el mensaje personalizado
        messageText.text = dialogMessage;

        // Configura la imagen de fondo si está asignada y no es un diálogo de solo mensaje
        Image backgroundImage = dialogInstance.transform.Find("DialogBackground")?.GetComponent<Image>();
        if (backgroundImage != null)
        {
            if (!showOnlyMessage && dialogBackgroundImage != null)
            {
                backgroundImage.sprite = dialogBackgroundImage; // Asigna la nueva imagen de fondo
                backgroundImage.gameObject.SetActive(true); // Activa la imagen de fondo
            }
            else
            {
                backgroundImage.gameObject.SetActive(false); // Desactiva la imagen de fondo si no es necesaria
            }
        }

        if (showOnlyMessage)
        {
            // Configura el diálogo para mostrar solo un botón "Claro"
            Button clearButton = dialogInstance.transform.Find("ClearButton")?.GetComponent<Button>();
            if (clearButton == null)
            {
                Debug.LogError("No se encontró el botón 'ClearButton' en el diálogo.");
                return;
            }

            // Configura el botón "Claro"
            clearButton.onClick.AddListener(OnClearClicked);

            // Desactiva los botones "Sí" y "No" si existen
            Button yesButton = dialogInstance.transform.Find("YesButton")?.GetComponent<Button>();
            Button noButton = dialogInstance.transform.Find("NoButton")?.GetComponent<Button>();
            if (yesButton != null) yesButton.gameObject.SetActive(false);
            if (noButton != null) noButton.gameObject.SetActive(false);
        }
        else
        {
            // Configura los botones "Sí" y "No"
            Button yesButton = dialogInstance.transform.Find("YesButton")?.GetComponent<Button>();
            Button noButton = dialogInstance.transform.Find("NoButton")?.GetComponent<Button>();

            if (yesButton == null || noButton == null)
            {
                Debug.LogError("No se encontraron los botones 'YesButton' o 'NoButton' en el diálogo.");
                return;
            }

            // Configura los botones
            yesButton.onClick.AddListener(OnYesClicked);
            noButton.onClick.AddListener(OnNoClicked);

            // Desactiva el botón "Claro" si existe
            Button clearButton = dialogInstance.transform.Find("ClearButton")?.GetComponent<Button>();
            if (clearButton != null) clearButton.gameObject.SetActive(false);
        }
    }

    private void OnYesClicked()
    {
        Debug.Log("Cambiando a la escena: " + sceneToLoad);
        if (playerMovement != null)
        {
            playerMovement.canMove = true; // Habilita el movimiento del jugador
        }
        SceneManager.LoadScene(sceneToLoad); // Carga la escena
        Destroy(dialogInstance); // Destruye el diálogo
    }

    private void OnNoClicked()
    {
        Debug.Log("El jugador decidió no jugar.");
        if (playerMovement != null)
        {
            playerMovement.canMove = true; // Habilita el movimiento del jugador
        }
        Destroy(dialogInstance); // Destruye el diálogo
    }

    private void OnClearClicked()
    {
        Debug.Log("El jugador entendió el mensaje.");
        if (playerMovement != null)
        {
            playerMovement.canMove = true; // Habilita el movimiento del jugador
        }
        Destroy(dialogInstance); // Destruye el diálogo
    }
}