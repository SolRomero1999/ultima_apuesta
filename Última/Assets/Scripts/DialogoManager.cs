using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroDialogManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDialogPrefab;  
    [SerializeField] private GameObject bartenderDialogPrefab;  
    [SerializeField] private Sprite playerImage; 
    [SerializeField] private Sprite bartenderImage;  

    private GameObject currentDialogInstance;
    private Button clearButton;
    private int currentLineIndex = 0;
    private struct DialogLine
    {
        public string text;
        public GameObject dialogPrefab;
        public Sprite characterImage;
    }

    private DialogLine[] dialogLines;

    private void Start()
    {
        if (PlayerPrefs.GetString("PreviousScene") == "MainMenu")
        {
            dialogLines = new DialogLine[] {
                new DialogLine { text = "¿Dónde estoy...? Todo esto se siente extraño...", dialogPrefab = playerDialogPrefab, characterImage = playerImage },
                new DialogLine { text = "Recuerdo haber muerto... pero no sé cómo sucedió...", dialogPrefab = playerDialogPrefab, characterImage = playerImage },
                new DialogLine { text = "¿Qué está pasando aquí?", dialogPrefab = playerDialogPrefab, characterImage = playerImage },
                new DialogLine { text = "Efectivamente, has muerto.", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
                new DialogLine { text = "Y ahora estás aquí, en este lugar entre la vida y la muerte.", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
                new DialogLine { text = "Para poder seguir adelante, tendrás que demostrar tu valía en una serie de juegos.", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
                new DialogLine { text = "Cada juego es una prueba. Si las superas, quizás encuentres respuestas.", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
                new DialogLine { text = "Si fallas... bueno, mejor ni hablemos de eso.", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
                new DialogLine { text = "¿Listo para empezar?", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage }
            };

            PlayerMovement player = FindObjectOfType<PlayerMovement>();
            if (player != null)
            {
                player.canMove = false;
            }

            ShowNextDialog();  
        }
    }

    private void ShowNextDialog()
    {
        if (currentLineIndex >= dialogLines.Length)
        {
            EndDialog();
            return;
        }

        if (currentDialogInstance != null)
        {
            Destroy(currentDialogInstance);  
        }

        currentDialogInstance = Instantiate(dialogLines[currentLineIndex].dialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
        currentDialogInstance.SetActive(true);

        TMP_Text messageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = dialogLines[currentLineIndex].text;
        }

        Image characterImage = currentDialogInstance.GetComponentInChildren<Image>();
        if (characterImage != null)
        {
            characterImage.sprite = dialogLines[currentLineIndex].characterImage;
        }

        clearButton = currentDialogInstance.GetComponentInChildren<Button>();
        if (clearButton != null)
        {
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() =>
            {
                currentLineIndex++;
                ShowNextDialog();
            });
        }
    }

    private void EndDialog()
    {
        Destroy(currentDialogInstance);  
        PlayerPrefs.DeleteKey("PreviousScene");
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canMove = true;
        }
    }
}
