using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

public class DialogManager : MonoBehaviour
{
    [Header("Dialog Prefabs")]
    [SerializeField] private GameObject playerDialogPrefab;
    [SerializeField] private GameObject secondPlayerDialogPrefab;
    [SerializeField] private GameObject bartenderDialogPrefab;
    [SerializeField] private GameObject secondBartenderDialogPrefab;
    [SerializeField] private GameObject choiceDialogPrefab;

    [Header("Text Settings")]
    [SerializeField] private float textSpeed = 0.05f; // Velocidad de escritura del texto

    private GameObject currentDialogInstance;
    private Button currentButton;
    private int currentLineIndex = 0;
    private bool isPostRuleta = false;
    private DialogLine[] currentDialogLines;
    private PlayerMovement cachedPlayerMovement;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private TMP_Text currentMessageText;

    private struct DialogLine
    {
        public string text;
        public GameObject dialogPrefab;
    }

    private void Start()
    {
        cachedPlayerMovement = FindObjectOfType<PlayerMovement>();
        InitializeDialogSystem();
    }

    private void OnDestroy()
    {
        if (currentButton != null)
        {
            currentButton.onClick.RemoveAllListeners();
        }
        
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
    }

    private void InitializeDialogSystem()
    {
        if (GameManager.Instance == null)
        {
            EnablePlayerMovement();
            return;
        }

        bool shouldStartDialog = false;

        if (GameManager.Instance.GetPlayerState() > 0)
        {
            InitializeNewPlayerDialog();
            shouldStartDialog = true;
        }
        else if (PlayerPrefs.GetString("PreviousScene") == "MainMenu")
        {
            InitializeIntroDialog();
            shouldStartDialog = true;
        }
        else if (GameManager.Instance.IsMinigameCompleted("Ruleta_Rusa") && 
                !GameManager.Instance.HasSeenPostRuletaDialogs())
        {
            InitializePostRuletaDialog();
            shouldStartDialog = true;
        }

        if (shouldStartDialog)
        {
            DisablePlayerMovement();
            ShowNextDialog();
        }
        else
        {
            EnablePlayerMovement();
        }
    }

    #region Dialog Initialization

    private void InitializeIntroDialog()
    {
        currentDialogLines = new DialogLine[]
        {
            CreateDialogLine("¿Dónde estoy...? Todo esto se siente extraño...", playerDialogPrefab),
            CreateDialogLine("Recuerdo haber muerto... pero no sé cómo sucedió...", playerDialogPrefab),
            CreateDialogLine("¿Qué está pasando aquí?", playerDialogPrefab),
            CreateDialogLine("Efectivamente, has muerto.", bartenderDialogPrefab),
            CreateDialogLine("Y ahora estás aquí, en este lugar entre la vida y la muerte.", bartenderDialogPrefab),
            CreateDialogLine("Para poder seguir adelante, tendrás que demostrar tu valía en una serie de juegos.", bartenderDialogPrefab),
            CreateDialogLine("Cada juego es una prueba. Si las superas, quizás encuentres respuestas.", bartenderDialogPrefab),
            CreateDialogLine("Si fallas... bueno, mejor ni hablemos de eso.", bartenderDialogPrefab),
            CreateDialogLine("¿Listo para empezar?", bartenderDialogPrefab)
        };
    }

    private void InitializePostRuletaDialog()
    {
        isPostRuleta = true;
        currentDialogLines = new DialogLine[]
        {
            CreateDialogLine("¿Creías que había muerto?", bartenderDialogPrefab),
            CreateDialogLine("En este lugar se podría decir que soy como un juez, una bala no me matará", bartenderDialogPrefab),
            CreateDialogLine("Aún así me has ganado", bartenderDialogPrefab),
            CreateDialogLine("¿Qué harás ahora?", bartenderDialogPrefab),
            CreateDialogLine("¿Qué haré? ¿Qué opciones tengo?", playerDialogPrefab),
            CreateDialogLine("Bueno, puedes reencarnar, olvidarás todo esto y podrás llevar una nueva vida", bartenderDialogPrefab),
            CreateDialogLine("O puedes liberarme y tomar mi lugar", bartenderDialogPrefab),
            CreateDialogLine("Ser el nuevo juez", bartenderDialogPrefab),
            CreateDialogLine("¿Qué prefieres?", bartenderDialogPrefab)
        };
    }

    private void InitializeNewPlayerDialog()
    {
        int playerState = GameManager.Instance.GetPlayerState();
        int judgeLevel = GameManager.Instance.GetCurrentJudgeLevel();
        GameObject bartenderPrefab = judgeLevel == 0 ? bartenderDialogPrefab : secondBartenderDialogPrefab;

        List<DialogLine> lines = new List<DialogLine>
        {
            CreateDialogLine("¿Dónde estoy?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Se podría decir que en el limbo, haz muerto" : 
                "Oh, al fin llegó alguien... estás en el limbo", bartenderPrefab),
            CreateDialogLine("¿El limbo? ¿Cómo llegué aquí?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Eso es obvio, haz muerto" : 
                "Bueno, lamentablemente haz muerto", bartenderPrefab),
            CreateDialogLine("¿Muerto? No lo recuerdo...", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Quizás lo hagas una vez que demuestres tu valor... supera los juegos aquí establecidos" : 
                "Es normal, yo tampoco lo hice. Lo recordarás a medida que vayas jugando. Si terminas los juegos recordarás y podrás irte", bartenderPrefab),
            CreateDialogLine("¿Debo jugar?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Sí, mejor comienza ya" : 
                "Sí, buena suerte", bartenderPrefab)
        };

        currentDialogLines = lines.ToArray();
    }

    private DialogLine CreateDialogLine(string text, GameObject prefab)
    {
        return new DialogLine { text = text, dialogPrefab = prefab };
    }

    #endregion

    #region Dialog Flow Control

    private void ShowNextDialog()
    {
        // Si está escribiendo, completar el texto inmediatamente
        if (isTyping)
        {
            CompleteText();
            return;
        }

        if (currentLineIndex >= currentDialogLines.Length)
        {
            HandleDialogEnd();
            return;
        }

        CleanupCurrentDialog();

        CreateDialogInstance(currentDialogLines[currentLineIndex]);
        currentLineIndex++;
    }

    private void CleanupCurrentDialog()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;

        if (currentDialogInstance != null)
        {
            Destroy(currentDialogInstance);
            currentDialogInstance = null;
        }

        if (currentButton != null)
        {
            currentButton.onClick.RemoveAllListeners();
            currentButton = null;
        }
    }

    private void CreateDialogInstance(DialogLine dialogLine)
    {
        currentDialogInstance = Instantiate(dialogLine.dialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GetCanvasTransform(), false);
        currentDialogInstance.SetActive(true);

        currentMessageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (currentMessageText != null)
        {
            typingCoroutine = StartCoroutine(TypeText(dialogLine.text));
        }

        SetupDialogButton(currentDialogInstance);
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentMessageText.text = "";
        
        foreach (char letter in text.ToCharArray())
        {
            currentMessageText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
        typingCoroutine = null;
    }

    private void CompleteText()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (currentMessageText != null && currentLineIndex > 0)
        {
            // Mostrar el texto completo de la línea actual
            currentMessageText.text = currentDialogLines[currentLineIndex - 1].text;
        }

        isTyping = false;
    }

    private Transform GetCanvasTransform()
    {
        GameObject canvas = GameObject.Find("Canvas");
        return canvas != null ? canvas.transform : null;
    }

    private void SetupDialogButton(GameObject dialogInstance)
    {
        currentButton = dialogInstance.GetComponentInChildren<Button>();
        if (currentButton != null)
        {
            currentButton.onClick.AddListener(ShowNextDialog);
        }
    }

    private void HandleDialogEnd()
    {
        if (isPostRuleta)
        {
            ShowFinalChoice();
        }
        else
        {
            EndDialog();
        }
    }

    #endregion

    #region Final Choice Handling

    private void ShowFinalChoice()
    {
        CleanupCurrentDialog();

        currentDialogInstance = Instantiate(choiceDialogPrefab, Vector3.zero, Quaternion.identity);
        currentDialogInstance.transform.SetParent(GetCanvasTransform(), false);
        currentDialogInstance.SetActive(true);

        currentMessageText = currentDialogInstance.GetComponentInChildren<TMP_Text>();
        if (currentMessageText != null)
        {
            currentMessageText.text = "¿Qué prefieres?";
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

        GameManager.Instance?.MarkPostRuletaDialogsSeen();
    }

    private void OnReencarnarClicked()
    {
        SceneManager.LoadScene("EndGame");
    }

    private void OnJuezClicked()
    {
        if (GameManager.Instance != null)
        {
            int playerState = GameManager.Instance.GetPlayerState();
            GameManager.Instance.SetNewJudge(playerState);
            GameManager.Instance.SetPlayerAsJudge();
        }

        UpdateBartenderState();
        SceneManager.LoadScene("EndGame");
    }

    private void UpdateBartenderState()
    {
        BartenderController bartender = FindObjectOfType<BartenderController>();
        bartender?.SetAlternateState();
    }

    #endregion

    #region Utility Methods

    private void EndDialog()
    {
        CleanupCurrentDialog();
        PlayerPrefs.DeleteKey("PreviousScene");
        EnablePlayerMovement();
    }

    private void DisablePlayerMovement()
    {
        if (cachedPlayerMovement != null)
        {
            cachedPlayerMovement.canMove = false;
        }
    }

    private void EnablePlayerMovement()
    {
        if (cachedPlayerMovement != null)
        {
            cachedPlayerMovement.canMove = true;
        }
    }

    #endregion
}