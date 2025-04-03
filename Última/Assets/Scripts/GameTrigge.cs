using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTrigger : MonoBehaviour
{
    #region Inspector Variables
    [Header("Scene Settings")]
    [SerializeField] private string[] scenesToLoad = new string[3];
    
    [Header("Dialog Settings")]
    [SerializeField] private GameObject dialogPrefab;
    [TextArea(3, 10)]
    [SerializeField] private string[] dialogMessages = new string[3];
    [SerializeField] private Sprite[] dialogBackgrounds = new Sprite[3];
    [SerializeField] private bool[] showOnlyMessageStates = new bool[3];
    
    [Header("Special Message")]
    [SerializeField] private bool isSpecialMessage = false;

    [Header("Visual Settings")]
    [SerializeField] private Sprite[] stateSprites = new Sprite[3];
    [SerializeField] private SpriteRenderer spriteRenderer;
    #endregion

    #region Private Variables
    private GameObject dialogInstance;
    private PlayerMovement playerMovement;
    private int currentState;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        InitializeState();
        UpdateVisuals();
    }

    private void OnDestroy()
    {
        CleanUpDialog();
    }
    #endregion

    #region Initialization
    private void InitializeState()
    {
        currentState = GameManager.Instance?.GetPlayerState() ?? 0;
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null && currentState < stateSprites.Length)
        {
            spriteRenderer.sprite = stateSprites[currentState];
        }
    }
    #endregion

    #region Dialog Management
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            HandlePlayerInteraction(other);
        }
    }

    private void HandlePlayerInteraction(Collider2D playerCollider)
    {
        playerMovement = playerCollider.GetComponent<PlayerMovement>();
        playerMovement?.SetMovementEnabled(false);
        ShowDialog();
    }

    private void ShowDialog()
    {
        if (dialogPrefab == null)
        {
            Debug.LogWarning("Dialog prefab is not assigned!");
            return;
        }

        CleanUpDialog();
        CreateDialogInstance();
    }

    private void CreateDialogInstance()
    {
        Transform canvasTransform = GameObject.Find("Canvas")?.transform;
        if (canvasTransform == null)
        {
            Debug.LogError("Canvas not found in scene!");
            return;
        }

        dialogInstance = Instantiate(dialogPrefab, canvasTransform);
        dialogInstance.SetActive(true);
        ConfigureDialog();
    }

    private void ConfigureDialog()
    {
        TMP_Text messageText = dialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText == null)
        {
            Debug.LogError("No TMP_Text component found in dialog!");
            return;
        }

        ConfigureDialogBackground();
        SetupDialogButtons(messageText);
    }

    private void ConfigureDialogBackground()
    {
        Image background = dialogInstance.transform.Find("DialogBackground")?.GetComponent<Image>();
        if (background != null && currentState < dialogBackgrounds.Length)
        {
            background.sprite = dialogBackgrounds[currentState];
            background.gameObject.SetActive(background.sprite != null);
        }
    }
    #endregion

    #region Button Setup
    private void SetupDialogButtons(TMP_Text messageText)
    {
        Button yesButton = GetDialogButton("YesButton");
        Button noButton = GetDialogButton("NoButton");
        Button clearButton = GetDialogButton("ClearButton");

        ClearButtonListeners(yesButton, noButton, clearButton);

        if (ShouldShowSimpleMessage())
        {
            SetupSimpleDialog(messageText, clearButton, yesButton, noButton);
        }
        else if (isSpecialMessage)
        {
            SetupSpecialDialog(messageText, yesButton, noButton, clearButton);
        }
        else
        {
            SetupNormalDialog(messageText, yesButton, noButton, clearButton);
        }
    }

    private Button GetDialogButton(string buttonName)
    {
        Transform buttonTransform = dialogInstance.transform.Find(buttonName);
        return buttonTransform?.GetComponent<Button>();
    }

    private void ClearButtonListeners(params Button[] buttons)
    {
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    private bool ShouldShowSimpleMessage()
    {
        return currentState < showOnlyMessageStates.Length && 
               showOnlyMessageStates[currentState];
    }

    private void SetupSimpleDialog(TMP_Text text, Button clearBtn, Button yesBtn, Button noBtn)
    {
        text.text = GetCurrentMessage();
        SetupButton(clearBtn, CloseDialog);
        SetButtonActive(yesBtn, false);
        SetButtonActive(noBtn, false);
    }

    private void SetupSpecialDialog(TMP_Text text, Button yesBtn, Button noBtn, Button clearBtn)
    {
        text.text = GetCurrentMessage();
        SetupButton(yesBtn, OnYesClicked);
        SetupButton(noBtn, CloseDialog);
        SetButtonActive(clearBtn, false);
    }

    private void SetupNormalDialog(TMP_Text text, Button yesBtn, Button noBtn, Button clearBtn)
    {
        string scene = GetCurrentScene();
        
        if (GameManager.Instance != null && GameManager.Instance.IsMinigameCompleted(scene))
        {
            text.text = "¡Ya completaste este desafío! Ve al siguiente juego.";
            SetupButton(clearBtn, CloseDialog);
            SetButtonActive(yesBtn, false);
            SetButtonActive(noBtn, false);
        }
        else if (CanEnterMinigame())
        {
            text.text = GetCurrentMessage();
            SetupButton(yesBtn, OnYesClicked);
            SetupButton(noBtn, CloseDialog);
            SetButtonActive(clearBtn, false);
        }
        else
        {
            text.text = "Aún no estás listo... date una vuelta.";
            SetupButton(clearBtn, CloseDialog);
            SetButtonActive(yesBtn, false);
            SetButtonActive(noBtn, false);
        }
    }

    private void SetupButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
        {
            button.onClick.AddListener(action);
            button.gameObject.SetActive(true);
        }
    }

    private void SetButtonActive(Button button, bool active)
    {
        if (button != null)
        {
            button.gameObject.SetActive(active);
        }
    }
    #endregion

    #region Utility Methods
    private string GetCurrentMessage()
    {
        return currentState < dialogMessages.Length ? 
               dialogMessages[currentState] : "Mensaje no configurado";
    }

    private string GetCurrentScene()
    {
        return currentState < scenesToLoad.Length ? 
               scenesToLoad[currentState] : string.Empty;
    }

    private bool CanEnterMinigame()
    {
        return GameManager.Instance == null || 
               GameManager.Instance.CanPlay(GetCurrentScene());
    }
    #endregion

    #region Button Actions
    private void OnYesClicked()
    {
        string scene = GetCurrentScene();
        if (!string.IsNullOrEmpty(scene))
        {
            SceneManager.LoadScene(scene);
        }
        CloseDialog();
    }

    private void CloseDialog()
    {
        CleanUpDialog();
        RestorePlayerMovement();
    }

    private void CleanUpDialog()
    {
        if (dialogInstance != null)
        {
            Destroy(dialogInstance);
            dialogInstance = null;
        }
    }

    private void RestorePlayerMovement()
    {
        playerMovement?.SetMovementEnabled(true);
        playerMovement = null; // Clear reference
    }
    #endregion

    #region Public Methods
    public void RefreshState()
    {
        InitializeState();
        UpdateVisuals();
    }
    #endregion
}