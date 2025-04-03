using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameTrigger : MonoBehaviour
{
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

    private GameObject dialogInstance;
    private PlayerMovement playerMovement;
    private int currentState;

    private void Start()
    {
        currentState = GameManager.Instance?.GetPlayerState() ?? 0;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (spriteRenderer != null && currentState < stateSprites.Length)
        {
            spriteRenderer.sprite = stateSprites[currentState];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            playerMovement?.SetMovementEnabled(false);
            ShowDialog();
        }
    }

    private void ShowDialog()
    {
        if (dialogPrefab == null) return;

        dialogInstance = Instantiate(dialogPrefab, GameObject.Find("Canvas").transform);
        dialogInstance.SetActive(true);
        ConfigureDialog();
    }

    private void ConfigureDialog()
    {
        var messageText = dialogInstance.GetComponentInChildren<TMP_Text>();
        if (messageText == null) return;

        // Configurar fondo
        var background = dialogInstance.transform.Find("DialogBackground")?.GetComponent<Image>();
        if (background != null && currentState < dialogBackgrounds.Length)
        {
            background.sprite = dialogBackgrounds[currentState];
            background.gameObject.SetActive(background.sprite != null);
        }

        // Obtener botones
        var yesButton = dialogInstance.transform.Find("YesButton")?.GetComponent<Button>();
        var noButton = dialogInstance.transform.Find("NoButton")?.GetComponent<Button>();
        var clearButton = dialogInstance.transform.Find("ClearButton")?.GetComponent<Button>();

        // Limpiar listeners previos
        if (yesButton != null) yesButton.onClick.RemoveAllListeners();
        if (noButton != null) noButton.onClick.RemoveAllListeners();
        if (clearButton != null) clearButton.onClick.RemoveAllListeners();

        // Mostrar según el modo
        bool showOnlyMessage = currentState < showOnlyMessageStates.Length && showOnlyMessageStates[currentState];
        
        if (showOnlyMessage)
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

    private void SetupSimpleDialog(TMP_Text text, Button clearBtn, Button yesBtn, Button noBtn)
    {
        text.text = GetCurrentMessage();
        if (clearBtn != null)
        {
            clearBtn.onClick.AddListener(CloseDialog);
            clearBtn.gameObject.SetActive(true);
        }
        SetButtonActive(yesBtn, false);
        SetButtonActive(noBtn, false);
    }

    private void SetupSpecialDialog(TMP_Text text, Button yesBtn, Button noBtn, Button clearBtn)
    {
        text.text = GetCurrentMessage();
        if (yesBtn != null)
        {
            yesBtn.onClick.AddListener(OnYesClicked);
            yesBtn.gameObject.SetActive(true);
        }
        if (noBtn != null)
        {
            noBtn.onClick.AddListener(CloseDialog);
            noBtn.gameObject.SetActive(true);
        }
        SetButtonActive(clearBtn, false);
    }

    private void SetupNormalDialog(TMP_Text text, Button yesBtn, Button noBtn, Button clearBtn)
    {
        string scene = GetCurrentScene();
        
        if (GameManager.Instance != null && GameManager.Instance.IsMinigameCompleted(scene))
        {
            text.text = "¡Ya completaste este desafío! Ve al siguiente juego.";
            if (clearBtn != null)
            {
                clearBtn.onClick.AddListener(CloseDialog);
                clearBtn.gameObject.SetActive(true);
            }
            SetButtonActive(yesBtn, false);
            SetButtonActive(noBtn, false);
        }
        else if (CanEnterMinigame())
        {
            text.text = GetCurrentMessage();
            if (yesBtn != null)
            {
                yesBtn.onClick.AddListener(OnYesClicked);
                yesBtn.gameObject.SetActive(true);
            }
            if (noBtn != null)
            {
                noBtn.onClick.AddListener(CloseDialog);
                noBtn.gameObject.SetActive(true);
            }
            SetButtonActive(clearBtn, false);
        }
        else
        {
            text.text = "Aún no estás listo... date una vuelta.";
            if (clearBtn != null)
            {
                clearBtn.onClick.AddListener(CloseDialog);
                clearBtn.gameObject.SetActive(true);
            }
            SetButtonActive(yesBtn, false);
            SetButtonActive(noBtn, false);
        }
    }

    private string GetCurrentMessage()
    {
        return currentState < dialogMessages.Length ? dialogMessages[currentState] : "";
    }

    private string GetCurrentScene()
    {
        return currentState < scenesToLoad.Length ? scenesToLoad[currentState] : "";
    }

    private bool CanEnterMinigame()
    {
        return GameManager.Instance == null || GameManager.Instance.CanPlay(GetCurrentScene());
    }

    private void SetButtonActive(Button button, bool active)
    {
        if (button != null) button.gameObject.SetActive(active);
    }

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
        if (dialogInstance != null)
        {
            Destroy(dialogInstance);
            dialogInstance = null;
        }
        playerMovement?.SetMovementEnabled(true);
    }
}