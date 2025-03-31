using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;  
    [SerializeField] private string dialogMessage; 
    [SerializeField] private GameObject dialogPrefab; 
    [SerializeField] private bool showOnlyMessage = false; 
    [SerializeField] private Sprite dialogBackgroundImage; 
    [SerializeField] private bool isSpecialMessage = false;  // Nueva variable para manejar el caso especial

    private GameObject dialogInstance; 
    private PlayerMovement playerMovement; 

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            playerMovement = other.GetComponent<PlayerMovement>(); 
            if (playerMovement != null)
            {
                playerMovement.StopMovement();
                playerMovement.canMove = false; 
            }

            ShowDialog();
        }
    }

    private void ShowDialog()
    {
        if (dialogPrefab == null)
        {
            Debug.LogError("No se ha asignado un prefab de diálogo.");
            return;
        }

        dialogInstance = Instantiate(dialogPrefab, Vector3.zero, Quaternion.identity);
        dialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
        dialogInstance.SetActive(true);

        TMP_Text messageText = dialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText == null)
        {
            Debug.LogError("No se encontró un componente TMP_Text en el diálogo.");
            return;
        }

        // Configurar imagen de fondo si existe
        Image backgroundImage = dialogInstance.transform.Find("DialogBackground")?.GetComponent<Image>();
        if (backgroundImage != null && dialogBackgroundImage != null)
        {
            backgroundImage.sprite = dialogBackgroundImage;
            backgroundImage.gameObject.SetActive(true);
        }

        // Obtener referencias a los botones
        Button yesButton = dialogInstance.transform.Find("YesButton")?.GetComponent<Button>();
        Button noButton = dialogInstance.transform.Find("NoButton")?.GetComponent<Button>();
        Button clearButton = dialogInstance.transform.Find("ClearButton")?.GetComponent<Button>();

        if (showOnlyMessage) 
        {
            // MODO SOLO MENSAJE (para mesas vacías)
            messageText.text = dialogMessage;

            if (clearButton != null)
            {
                clearButton.onClick.AddListener(OnClearClicked);
                clearButton.gameObject.SetActive(true);
            }
            
            if (yesButton != null) yesButton.gameObject.SetActive(false);
            if (noButton != null) noButton.gameObject.SetActive(false);
        }
        else if (isSpecialMessage) 
        {
            // MODO MENSAJE ESPECIAL (para casos especiales, con botones Sí/No)
            messageText.text = dialogMessage;
            
            if (yesButton != null)
            {
                yesButton.onClick.AddListener(OnYesClicked);
                yesButton.gameObject.SetActive(true);
            }
            
            if (noButton != null)
            {
                noButton.onClick.AddListener(OnNoClicked);
                noButton.gameObject.SetActive(true);
            }

            if (clearButton != null) clearButton.gameObject.SetActive(false);
        }
        else
        {
            // MODO JUEGO (para bar, par/impar, etc.)
            if (CanEnterMinigame())
            {
                // Puede jugar - mostrar mensaje normal y botones Sí/No
                messageText.text = dialogMessage;
                
                if (yesButton != null) 
                {
                    yesButton.onClick.AddListener(OnYesClicked);
                    yesButton.gameObject.SetActive(true);
                }
                
                if (noButton != null) 
                {
                    noButton.onClick.AddListener(OnNoClicked);
                    noButton.gameObject.SetActive(true);
                }
                
                if (clearButton != null) clearButton.gameObject.SetActive(false);
            }
            else
            {
                // No puede jugar aún - mostrar mensaje de "no listo" y solo botón Claro
                messageText.text = "Aún no estás listo... date una vuelta.";
                
                if (clearButton != null)
                {
                    clearButton.onClick.AddListener(OnClearClicked);
                    clearButton.gameObject.SetActive(true);
                }
                
                if (yesButton != null) yesButton.gameObject.SetActive(false);
                if (noButton != null) noButton.gameObject.SetActive(false);
            }
        }
    }

    private bool CanEnterMinigame()
    {
        if (GameManager.instance == null) return true;
        return GameManager.instance.CanPlay(sceneToLoad);
    }

    private void OnYesClicked()
    {
        if (playerMovement != null)
        {
            playerMovement.canMove = true; 
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.AdvanceProgress(sceneToLoad);
        }

        SceneManager.LoadScene(sceneToLoad); 
        Destroy(dialogInstance); 
    }

    private void OnNoClicked()
    {
        if (playerMovement != null)
        {
            playerMovement.canMove = true; 
        }
        Destroy(dialogInstance); 
    }

    private void OnClearClicked()
    {
        if (playerMovement != null)
        {
            playerMovement.canMove = true; 
        }
        Destroy(dialogInstance);
    }
}
