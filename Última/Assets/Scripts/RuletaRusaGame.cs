using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RuletaRusa : MonoBehaviour
{
    #region UI References
    public GameObject rulesPanel;
    public Button startButton;
    public TMP_Text rulesText; 
    public TMP_Text dealerText;
    public GameObject shootPanel;
    public Button dispararDealerButton;
    public Button dispararseButton;
    public Button girarBarrilButton;
    public Image backgroundImage;
    public GameObject blackScreen;
    public GameObject dialoguePanel;
    #endregion

    #region Visual Assets
    public Sprite imgInicial;
    public Sprite imgDealerGira;
    public Sprite imgDealerDispara;
    public Sprite imgDealerSuicida;
    public Sprite imgJugadorDispara;
    public Sprite imgJugadorSuicida;
    public Sprite imgJugadorGira;
    public Sprite imgDealerMuerto;
    #endregion

    #region Audio
    public AudioSource audioSource;
    public AudioClip disparoClip;
    public AudioClip disparoFallidoClip;
    public AudioClip girarClip;
    #endregion

    #region Game Variables
    private int[] barril = { 1, 2, 3, 4, 5, 6 };
    private int posicionActual;
    private int balaReal;
    private bool esTurnoDelJugador = false;
    private bool isQuitting = false;
    private bool buttonsBlocked = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    
    private string[] rulesDialogue = new string[]
    {
        "No nos compliquemos la vida, vayamos con algo sencillo",
        "Imagino que conoces la ruleta rusa, pero por las dudas te explicaré...",
        "Este revolver tiene 6 espacios en el barril, pero solo 1 bala",
        "Nos turnaremos para disparar",
        "Si te crees valiente puedes usar tu turno para dispararte a ti mismo",
        "Si vas con suerte y te salvas tienes otro turno",
        "Además tenemos la opción de girar el barril, pero de hacerlo pierdes tu turno",
        "Sencillo, ¿no?"
    };
    private int currentDialogueIndex = 0;
    private bool hasPlayedBefore = false;
    private Coroutine typingCoroutine;
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
        hasPlayedBefore = PlayerPrefs.GetInt("HasPlayedRuletaRusaBefore", 0) == 1;

        InitializeUI();
        SetupButtonListeners();
    }

    private void OnEnable()
    {
        isQuitting = false;
    }

    private void OnDisable()
    {
        if (!isQuitting)
        {
            CleanUp();
        }
    }

    private void OnDestroy()
    {
        isQuitting = true;
        CleanUp();
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
    #endregion

    #region Initialization
    private void InitializeUI()
    {
        rulesPanel.SetActive(true);
        dealerText.gameObject.SetActive(false);
        shootPanel.SetActive(false);
        backgroundImage.sprite = imgInicial;
        blackScreen.SetActive(false);
        dialoguePanel.SetActive(false);
        
        if (hasPlayedBefore)
        {
            rulesText.text = "Aquí de nuevo... bueno, empecemos";
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            rulesText.text = "";
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(AdvanceDialogue);
            StartCoroutine(TypeDialogueText(rulesDialogue[currentDialogueIndex]));
        }
    }

    private void SetupButtonListeners()
    {
        dispararDealerButton.onClick.AddListener(() => {
            if (!buttonsBlocked) StartCoroutine(Disparar(true));
        });
        dispararseButton.onClick.AddListener(() => {
            if (!buttonsBlocked) StartCoroutine(Disparar(false));
        });
        girarBarrilButton.onClick.AddListener(() => {
            if (!buttonsBlocked) StartCoroutine(GirarBarril());
        });
    }

    private void CleanUp()
    {
        StopAllCoroutines();
        
        if (startButton != null) startButton.onClick.RemoveAllListeners();
        if (dispararDealerButton != null) dispararDealerButton.onClick.RemoveAllListeners();
        if (dispararseButton != null) dispararseButton.onClick.RemoveAllListeners();
        if (girarBarrilButton != null) girarBarrilButton.onClick.RemoveAllListeners();

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
    #endregion

    #region Button Management
    private void SetButtonsInteractable(bool interactable)
    {
        dispararDealerButton.interactable = interactable;
        dispararseButton.interactable = interactable;
        girarBarrilButton.interactable = interactable;
        buttonsBlocked = !interactable;
    }
    #endregion

    #region Game Flow
    private void AdvanceDialogue()
    {
        if (isTyping && !skipTyping)
        {
            skipTyping = true;
            return;
        }

        skipTyping = false;
        currentDialogueIndex++;
        
        if (currentDialogueIndex < rulesDialogue.Length)
        {
            StartCoroutine(TypeDialogueText(rulesDialogue[currentDialogueIndex]));
        }
        else
        {
            PlayerPrefs.SetInt("HasPlayedRuletaRusaBefore", 1);
            StartGame();
        }
    }

    private IEnumerator TypeDialogueText(string text)
    {
        isTyping = true;
        rulesText.text = "";
        skipTyping = false;
        
        foreach (char letter in text.ToCharArray())
        {
            if (skipTyping)
            {
                rulesText.text = text;
                break;
            }
            
            rulesText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }
        
        isTyping = false;
    }

    private void StartGame()
    {
        rulesPanel.SetActive(false);
        dealerText.gameObject.SetActive(true);
        dialoguePanel.SetActive(true);
        dealerText.text = "Si no te molesta, empezaré yo";
        StartCoroutine(EsperarYEmpezar());
    }

    private void PosicionInicial()
    {
        posicionActual = Random.Range(0, barril.Length);
        balaReal = Random.Range(0, barril.Length);
    }

    private void AvanzarBarril()
    {
        posicionActual = (posicionActual + 1) % barril.Length;
    }
    #endregion

    #region Coroutines
    private IEnumerator EsperarYEmpezar()
    {
        yield return new WaitForSeconds(2f);
        PosicionInicial();
        StartCoroutine(DealerTurn());
    }

    private IEnumerator DealerTurn()
    {
        SetButtonsInteractable(false);
        esTurnoDelJugador = false;
        shootPanel.SetActive(false);
        backgroundImage.sprite = imgInicial;
        yield return new WaitForSeconds(2f);

        int decision = Random.Range(0, 100);

        if (decision < 50) 
        {
            backgroundImage.sprite = imgDealerDispara;
            dealerText.text = "Veamos si la suerte te acompaña...";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(Disparar(true));
        }
        else if (decision < 80) 
        {
            backgroundImage.sprite = imgDealerSuicida;
            dealerText.text = "Espero tener suerte...";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(Disparar(false));
        }
        else 
        {
            backgroundImage.sprite = imgDealerGira;
            dealerText.text = "Hagamoslo más interesante";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(GirarBarril());
        }
    }

    private IEnumerator GirarBarril()
    {
        SetButtonsInteractable(false);
        
        posicionActual = (posicionActual + 1) % barril.Length;
        PlaySound(girarClip);
        
        dealerText.text = "Haz que giren.";
        backgroundImage.sprite = esTurnoDelJugador ? imgJugadorGira : imgDealerGira;
        yield return new WaitForSeconds(2f);

        if (!esTurnoDelJugador)
        {
            esTurnoDelJugador = true;
            shootPanel.SetActive(true);
            backgroundImage.sprite = imgInicial;
            SetButtonsInteractable(true);
            yield return new WaitForSeconds(0.1f); 
        }
        else
        {
            yield return StartCoroutine(DealerTurn());
        }
    }

    private IEnumerator Disparar(bool dispararAlJugador)
    {
        SetButtonsInteractable(false);
        
        if (posicionActual == balaReal) 
        {
            if (dispararAlJugador)
            {
                if (esTurnoDelJugador)
                {            
                    backgroundImage.sprite = imgJugadorDispara;
                    yield return new WaitForSeconds(0.5f);
                    PlaySound(disparoClip);
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(2f);
                }
                else
                {
                    backgroundImage.sprite = imgJugadorSuicida;
                    PlaySound(disparoClip);
                    dealerText.text = "No es tu día...";
                    yield return new WaitForSeconds(1f);
                    yield return StartCoroutine(PlayerDeath());
                    yield break;
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    backgroundImage.sprite = imgJugadorSuicida;
                    PlaySound(disparoClip);
                    dealerText.text = "No eres muy listo...";
                    yield return new WaitForSeconds(1f);
                    yield return StartCoroutine(PlayerDeath());
                    yield break;
                }
                else
                {
                    PlaySound(disparoClip);
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(2f);
                }
            }
            yield return new WaitForSeconds(1f);
            StartCoroutine(EndGame());
        }
        else 
        {
            if (dispararAlJugador)
            {
                if (esTurnoDelJugador)
                {
                    PlaySound(disparoFallidoClip);
                    dealerText.text = "Ahora es mi turno...";
                    backgroundImage.sprite = imgJugadorDispara;
                    AvanzarBarril();
                    yield return new WaitForSeconds(2f);
                    yield return StartCoroutine(DealerTurn());
                }
                else
                {
                    PlaySound(disparoFallidoClip);
                    dealerText.text = "Vas con suerte...";
                    backgroundImage.sprite = imgDealerDispara;
                    AvanzarBarril();
                    esTurnoDelJugador = true;
                    shootPanel.SetActive(true);
                    backgroundImage.sprite = imgInicial;
                    yield return new WaitForSeconds(0.5f); 
                    SetButtonsInteractable(true);
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    backgroundImage.sprite = imgJugadorSuicida;
                    PlaySound(disparoFallidoClip);
                    dealerText.text = "Vas con suerte...";
                    AvanzarBarril();
                    esTurnoDelJugador = true;
                    shootPanel.SetActive(true);
                    backgroundImage.sprite = imgInicial;
                    yield return new WaitForSeconds(0.5f); 
                    SetButtonsInteractable(true);
                }
                else
                {
                    PlaySound(disparoFallidoClip);
                    dealerText.text = "Otra vez mi turno...";
                    backgroundImage.sprite = imgDealerSuicida;
                    AvanzarBarril();
                    yield return new WaitForSeconds(2f);
                    yield return StartCoroutine(DealerTurn());
                }
            }
        }
    }

    private IEnumerator PlayerDeath()
    {
        blackScreen.SetActive(true);
        yield return new WaitForSeconds(2f);
        if (AnomaliasController.Instance != null)
        {
            AnomaliasController.Instance.AddAnomalia();
        }
        PlayerPrefs.SetString("LastScene", "Ruleta_Rusa");
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator EndGame()
    {
        if (!isQuitting && GameManager.Instance != null)
        {
            GameManager.Instance.CompleteMinigame("Ruleta_Rusa");
        }
        if (AnomaliasController.Instance != null)
        {
            AnomaliasController.Instance.RemoveAnomalia();
        }
        
        yield return new WaitForSeconds(2f);
        PlayerPrefs.SetString("LastScene", "Ruleta_Rusa");
        SceneManager.LoadScene("MainScene");
    }
    #endregion

    #region Utility Methods
    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    #endregion
}