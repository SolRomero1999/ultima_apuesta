using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RuletaRusa : MonoBehaviour
{
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

    public Sprite imgInicial;        
    public Sprite imgDealerGira;     
    public Sprite imgDealerDispara;  
    public Sprite imgDealerSuicida;  
    public Sprite imgJugadorDispara;  
    public Sprite imgJugadorSuicida; 
    public Sprite imgJugadorGira;    
    public Sprite imgDealerMuerto;    
    public AudioSource audioSource;     
    public AudioClip disparoClip;   
    public AudioClip disparoFallidoClip;     
    public AudioClip girarClip;     

    private int[] barril = { 1, 2, 3, 4, 5, 6 }; 
    private int posicionActual; 
    private int balaReal;       
    private bool esTurnoDelJugador = false;
    private bool isQuitting = false;

    void Start()
    {
        rulesPanel.SetActive(true);
        dealerText.gameObject.SetActive(false);
        shootPanel.SetActive(false);
        backgroundImage.sprite = imgInicial; 
        blackScreen.SetActive(false); 
        dialoguePanel.SetActive(false); 

        startButton.onClick.AddListener(StartGame);
        dispararDealerButton.onClick.AddListener(() => StartCoroutine(Disparar(true)));
        dispararseButton.onClick.AddListener(() => StartCoroutine(Disparar(false)));
        girarBarrilButton.onClick.AddListener(() => StartCoroutine(GirarBarril()));
    }

    void OnEnable()
    {
        isQuitting = false;
    }

    void OnDisable()
    {
        if (!isQuitting)
        {
            CleanUp();
        }
    }

    void OnDestroy()
    {
        isQuitting = true;
        CleanUp();
    }

    void OnApplicationQuit()
    {
        isQuitting = true;
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

    void StartGame()
    {
        rulesPanel.SetActive(false);
        dealerText.gameObject.SetActive(true);
        dialoguePanel.SetActive(true); 
        dealerText.text = "Si no te molesta, empezaré yo";
        StartCoroutine(EsperarYEmpezar()); 
    }

    IEnumerator EsperarYEmpezar()
    {
        yield return new WaitForSeconds(2f); 
        PosicionInicial();
        StartCoroutine(DealerTurn()); 
    }

    void PosicionInicial()
    {
        posicionActual = Random.Range(0, barril.Length);
        balaReal = Random.Range(0, barril.Length);
    }

    IEnumerator DealerTurn()
    {
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

    IEnumerator GirarBarril()
    {
        posicionActual = (posicionActual + 1) % barril.Length;
        if (audioSource != null && girarClip != null)
        {
            audioSource.PlayOneShot(girarClip);
        }
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

    IEnumerator Disparar(bool dispararAlJugador)
    {
        if (posicionActual == balaReal) 
        {
            if (dispararAlJugador)
            {
                if (esTurnoDelJugador)
                {
                    if (audioSource != null && disparoClip != null)
                    {
                        audioSource.PlayOneShot(disparoClip);
                    }
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(2f); 
                }
                else
                {
                    backgroundImage.sprite = imgJugadorSuicida; 
                    if (audioSource != null && disparoClip != null)
                    {
                        audioSource.PlayOneShot(disparoClip);
                    }
                    dealerText.text = "No es tu día...";
                    yield return StartCoroutine(PlayerDeath()); 
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    backgroundImage.sprite = imgJugadorSuicida; 
                    if (audioSource != null && disparoClip != null)
                    {
                        audioSource.PlayOneShot(disparoClip);
                    }
                    dealerText.text = "No eres muy listo...";
                    yield return new WaitForSeconds(2f); 
                    yield return StartCoroutine(PlayerDeath()); 
                }
                else
                {
                    if (audioSource != null && disparoClip != null)
                    {
                        audioSource.PlayOneShot(disparoClip);
                    }
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(2f); 
                }
            }
            yield return new WaitForSeconds(2f); 
            StartCoroutine(EndGame());
        }
        else
        {
            if (dispararAlJugador)
            {
                if (esTurnoDelJugador)
                {
                    if (audioSource != null && disparoFallidoClip != null)
                    {
                        audioSource.PlayOneShot(disparoFallidoClip);
                    }
                    dealerText.text = "Ahora es mi turno...";
                    backgroundImage.sprite = imgJugadorDispara;
                    AvanzarBarril();
                    yield return new WaitForSeconds(2f); 
                    yield return StartCoroutine(DealerTurn());
                }
                else
                {
                    if (audioSource != null && disparoFallidoClip != null)
                    {
                        audioSource.PlayOneShot(disparoFallidoClip);
                    }
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
                    if (audioSource != null && disparoFallidoClip != null)
                    {
                        audioSource.PlayOneShot(disparoFallidoClip);
                    }
                    dealerText.text = "Vas con suerte...";
                    AvanzarBarril();
                    esTurnoDelJugador = true;
                    shootPanel.SetActive(true);
                    backgroundImage.sprite = imgInicial;
                    yield return new WaitForSeconds(2f); 
                }
                else
                {
                    if (audioSource != null && disparoFallidoClip != null)
                    {
                        audioSource.PlayOneShot(disparoFallidoClip);
                    }
                    dealerText.text = "Otra vez mi turno...";
                    backgroundImage.sprite = imgDealerSuicida;
                    AvanzarBarril();
                    yield return new WaitForSeconds(2f); 
                    yield return StartCoroutine(DealerTurn());
                }
            }
        }
    }

    void AvanzarBarril()
    {
        posicionActual = (posicionActual + 1) % barril.Length;
    }

    IEnumerator PlayerDeath()
    {
        blackScreen.SetActive(true); 
        yield return new WaitForSeconds(2f); 
        SceneManager.LoadScene("MainScene"); 
    }

    IEnumerator EndGame()
    {
        if (!isQuitting && GameManager.instance != null)
        {
            GameManager.instance.CompleteMinigame("Ruleta_Rusa");
        }
        
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainScene");
    }
}