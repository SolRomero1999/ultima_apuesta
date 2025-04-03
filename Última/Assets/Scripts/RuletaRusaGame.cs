using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RuletaRusa : MonoBehaviour
{
    #region UI References
    [Header("UI References")]
    public GameObject rulesPanel;
    public Button startButton;
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
    [Header("Visual Assets")]
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
    [Header("Audio")]
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
    #endregion

    #region Unity Lifecycle Methods
    private void Start()
    {
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
    }

    private void SetupButtonListeners()
    {
        startButton.onClick.AddListener(StartGame);
        dispararDealerButton.onClick.AddListener(() => StartCoroutine(Disparar(true)));
        dispararseButton.onClick.AddListener(() => StartCoroutine(Disparar(false)));
        girarBarrilButton.onClick.AddListener(() => StartCoroutine(GirarBarril()));
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

    #region Game Flow
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
        esTurnoDelJugador = false;
        shootPanel.SetActive(false);
        backgroundImage.sprite = imgInicial;
        yield return new WaitForSeconds(2f);

        int decision = Random.Range(0, 100);

        if (decision < 50) // Disparar al jugador (50%)
        {
            backgroundImage.sprite = imgDealerDispara;
            dealerText.text = "Veamos si la suerte te acompaña...";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(Disparar(true));
        }
        else if (decision < 80) // Dispararse a sí mismo (30%)
        {
            backgroundImage.sprite = imgDealerSuicida;
            dealerText.text = "Espero tener suerte...";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(Disparar(false));
        }
        else // Girar el barril (20%)
        {
            backgroundImage.sprite = imgDealerGira;
            dealerText.text = "Hagamoslo más interesante";
            yield return new WaitForSeconds(2f);
            yield return StartCoroutine(GirarBarril());
        }
    }

    private IEnumerator GirarBarril()
    {
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
            yield return new WaitForSeconds(2f);
        }
        else
        {
            yield return StartCoroutine(DealerTurn());
        }
    }

    private IEnumerator Disparar(bool dispararAlJugador)
    {
        if (posicionActual == balaReal) // Bala real
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
                    yield return StartCoroutine(PlayerDeath());
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    backgroundImage.sprite = imgJugadorSuicida;
                    PlaySound(disparoClip);
                    dealerText.text = "No eres muy listo...";
                    yield return new WaitForSeconds(2f);
                    yield return StartCoroutine(PlayerDeath());
                }
                else
                {
                    PlaySound(disparoClip);
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(2f);
                }
            }
            yield return new WaitForSeconds(2f);
            StartCoroutine(EndGame());
        }
        else // Bala falsa
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
                    yield return new WaitForSeconds(2f);
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
                    yield return new WaitForSeconds(2f);
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
        PlayerPrefs.SetString("LastScene", "Ruleta_Rusa");
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator EndGame()
    {
        if (!isQuitting && GameManager.Instance != null)
        {
            GameManager.Instance.CompleteMinigame("Ruleta_Rusa");
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