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
            CreateDialogLine("¿Dónde estoy...? Todo esto se siente... irreal.", playerDialogPrefab),
            CreateDialogLine("Recuerdo haber muerto... pero el final fue tan confuso, tan abrupto...", playerDialogPrefab),
            CreateDialogLine("¿Qué es este lugar?", playerDialogPrefab),
            CreateDialogLine("Has cruzado el umbral. Estás entre lo que fue... y lo que podría ser.", bartenderDialogPrefab),
            CreateDialogLine("Aquí, en este limbo, los recuerdos se desvanecen y el juicio comienza.", bartenderDialogPrefab),
            CreateDialogLine("Para avanzar, deberás enfrentar una serie de juegos. No son simples desafíos... son ecos de tu alma.", bartenderDialogPrefab),
            CreateDialogLine("Cada uno pondrá a prueba tu voluntad e ingenio...", bartenderDialogPrefab),
            CreateDialogLine("Si fallas, podrías perderte para siempre. Pero si persistes, si no olvidas quién eres al jugar una y otra vez...", bartenderDialogPrefab),
            CreateDialogLine("...entonces tal vez, solo tal vez, merezcas una segunda oportunidad.", bartenderDialogPrefab),
            CreateDialogLine("Así que dime, viajero de la muerte... ¿estás listo para empezar?", bartenderDialogPrefab),
            CreateDialogLine("Tu primer prueba te espera en el rincón más colorido del olvido donde deberás probar tu suerte", bartenderDialogPrefab),
        };
    }

    private void InitializePostRuletaDialog()
    {
        isPostRuleta = true;
        currentDialogLines = new DialogLine[]
        {
            CreateDialogLine("¿De verdad creías que podías matarme?", bartenderDialogPrefab),
            CreateDialogLine("Aquí no soy un hombre... soy un juicio encarnado. Una bala no basta para detener lo que represento.", bartenderDialogPrefab),
            CreateDialogLine("Y sin embargo, lo lograste. Me venciste.", bartenderDialogPrefab),
            CreateDialogLine("Ahora la decisión no es mía... es tuya.", bartenderDialogPrefab),
            CreateDialogLine("¿Mía? ¿Qué decisión podría tomar aquí?", playerDialogPrefab),
            CreateDialogLine("Puedes renacer. Volver a la rueda, olvidar este lugar... y comenzar de nuevo, con otra piel, otro nombre.", bartenderDialogPrefab),
            CreateDialogLine("O puedes quedarte. Romper el ciclo. Convertirte en lo que yo fui.", bartenderDialogPrefab),
            CreateDialogLine("Ser el nuevo juez... custodio de las almas perdidas, guardián del tránsito final.", bartenderDialogPrefab),
            CreateDialogLine("No hay respuestas correctas aquí. Solo caminos. ¿Cuál seguirás tú?", bartenderDialogPrefab)
        };
    }

    private void InitializeNewPlayerDialog()
    {
        int playerState = GameManager.Instance.GetPlayerState();
        int judgeLevel = GameManager.Instance.GetCurrentJudgeLevel();
        GameObject bartenderPrefab = judgeLevel == 0 ? bartenderDialogPrefab : secondBartenderDialogPrefab;

        List<DialogLine> lines = new List<DialogLine>
        {
            CreateDialogLine("¿Q-qué es este lugar...?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Podríamos llamarlo limbo. Has cruzado el umbral... has muerto." : 
                "Oh... llegó alguien. Bienvenido al limbo.", bartenderPrefab),
            CreateDialogLine("¿El limbo...? No entiendo... ¿cómo llegué aquí?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "No es difícil de imaginar. Tu historia llegó a su fin." : 
                "Lamentablemente, has muerto. Así llegan todos aquí.", bartenderPrefab),
            CreateDialogLine("¿Muerto...? No... esto no puede estar pasando...", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Este lugar no da segundas oportunidades sin pruebas. Si quieres salir de aquí, tendrás que superar los juegos." : 
                "Sí... pero no todo está perdido. Si completas las pruebas, puede que encuentres tu camino.", bartenderPrefab),
            CreateDialogLine("¿Debo jugar...? ¿Qué clase de juegos son?", secondPlayerDialogPrefab),
            CreateDialogLine(judgeLevel == 0 ? 
                "Juegos que revelan quién eres. Te aconsejo que empieces cuanto antes." : 
                "Son pruebas... no fáciles, pero necesarias. Buena suerte.", bartenderPrefab)
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
        CleanupCurrentDialog();
        SceneManager.LoadScene("EndGame");
    }

    private void OnJuezClicked()
    {
        CleanupCurrentDialog();
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