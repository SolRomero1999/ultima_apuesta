using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PostRuletaDialogManager : MonoBehaviour
{
    [SerializeField] private GameObject playerDialogPrefab;  
    [SerializeField] private GameObject bartenderDialogPrefab;  
    [SerializeField] private Sprite playerImage; 
    [SerializeField] private Sprite bartenderImage;
    [SerializeField] private GameObject choiceDialogPrefab;

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
        // Solo activar si el jugador completó la Ruleta Rusa
        if (GameManager.instance != null && GameManager.instance.IsMinigameCompleted("Ruleta_Rusa") && 
            !GameManager.instance.HasSeenPostRuletaDialogs())
        {
            InitializeDialogs();
            ShowNextDialog();
        }
    }

    private void InitializeDialogs()
    {
        dialogLines = new DialogLine[] {
            new DialogLine { text = "¿Creeías que había muerto?", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "En este lugar se podría decir que soy como un juez, una bala no me matará", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "Aún así me has ganado", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "¿Qué harás ahora?", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "¿Qué haré? ¿Qué opciones tengo?", dialogPrefab = playerDialogPrefab, characterImage = playerImage },
            new DialogLine { text = "Bueno, puedes reencarnar, olvidarás todo esto y podrás llevar una nueva vida", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "O puedes liberarme y tomar mi lugar", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "Ser el nuevo juez", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage },
            new DialogLine { text = "¿Qué prefieres?", dialogPrefab = bartenderDialogPrefab, characterImage = bartenderImage }
        };

        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canMove = false;
        }
    }

    private void ShowNextDialog()
    {
        if (currentLineIndex >= dialogLines.Length)
        {
            ShowFinalChoice();
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

    private void ShowFinalChoice()
    {
        Destroy(currentDialogInstance);
        
        currentDialogInstance = Instantiate(choiceDialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GameObject.Find("Canvas").transform, false);
        currentDialogInstance.SetActive(true);

        TMP_Text messageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText != null)
        {
            messageText.text = "¿Qué prefieres?";
        }

        Image characterImage = currentDialogInstance.GetComponentInChildren<Image>();
        if (characterImage != null)
        {
            characterImage.sprite = bartenderImage;
        }

        Button reencarnarButton = currentDialogInstance.transform.Find("ReencarnarButton")?.GetComponent<Button>();
        Button juezButton = currentDialogInstance.transform.Find("JuezButton")?.GetComponent<Button>();

        if (reencarnarButton != null)
        {
            reencarnarButton.onClick.AddListener(OnReencarnarClicked);
        }
        
        if (juezButton != null)
        {
            juezButton.onClick.AddListener(OnJuezClicked);
        }

        // Marcar que ya se vieron estos diálogos
        if (GameManager.instance != null)
        {
            GameManager.instance.MarkPostRuletaDialogsSeen();
        }
    }

    private void OnReencarnarClicked()
    {
        // Lógica para reencarnar
        SceneManager.LoadScene("EndGame"); // O la escena que corresponda
    }

    private void OnJuezClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetPlayerAsJudge();
        }

        BartenderController bartender = FindObjectOfType<BartenderController>();
        if (bartender != null)
        {
            bartender.SetAlternateState(); // Cambiar sprite del bartender
        }

        SceneManager.LoadScene("EndGame");
    }


    private void EndDialog()
    {
        Destroy(currentDialogInstance);
        PlayerMovement player = FindObjectOfType<PlayerMovement>();
        if (player != null)
        {
            player.canMove = true;
        }
    }
}