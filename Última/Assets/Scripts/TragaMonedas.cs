using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class TragaMonedas : MonoBehaviour
{
    #region Configuración Visual
    [Header("Configuración Visual")]
    [SerializeField] private Sprite machineNormal;
    [SerializeField] private Sprite machineLeverDown;
    [SerializeField] private Sprite machineWin;    
    [SerializeField] private Sprite machineLose;  
    [SerializeField] private Sprite[] symbols;
    #endregion

    #region Componentes UI
    [Header("Componentes UI")]
    [SerializeField] private Image machineImage;
    [SerializeField] private Image[] slots;
    [SerializeField] private Button leverButton;
    [SerializeField] private GameObject winPanel; 
    [SerializeField] private TMP_Text winText;
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private TMP_Text rulesText;
    [SerializeField] private Button startButton;
    #endregion

    #region Configuración Tiempos
    [Header("Configuración Tiempos")]
    [SerializeField] private float leverDownTime = 0.5f;
    [SerializeField] private float totalSpinTime = 1.5f;
    [SerializeField] private float initialSpinSpeed = 0.05f;
    [SerializeField] private float finalSpinSpeed = 0.2f;
    [SerializeField] private float winDelay = 0.5f; 
    [SerializeField] private float resultDisplayTime = 1.5f;
    [SerializeField] private float textSpeed = 0.05f;
    #endregion

    #region Configuración Dificultad
    [Header("Configuración Dificultad")]
    [Range(0, 100)] [SerializeField] private int probabilidadGanar = 15;
    [Range(0, 100)] [SerializeField] private int probabilidadMensajeDerrota = 50;
    #endregion

    #region Efectos de Sonido
    [Header("Efectos de Sonido")]
    [SerializeField] private AudioClip leverPullSound;
    [SerializeField] private AudioClip slotSpinSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioSource audioSource;
    #endregion

    #region Variables Privadas
    private string[] rulesDialogue = {
        "Bueno, lo normal seria tener un enfrentamiento o algo así",
        "Pero tengo cero ganas de jugar contigo",
        "Así que prueba suerte con esto",
        "Veámoslo como que estas llamando a la suerte para los siguientes juegos",
        "Ya que la necesitaras"
    };
    private string[] mensajesDerrota = { 
        "Mala Suerte", 
        "Quizá la próxima", 
        "Error 404: Premio no encontrado.", 
        "Hoy no, quizá nunca.", 
        "Si esto fuera casino, ya estarías en bancarrota."
    };

    private int[] currentResults = new int[3];
    private int currentDialogueIndex = 0;
    private bool isSpinning = false;
    private Coroutine spinCoroutine;
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private bool skipTyping = false;
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        InitializeGame();
    }

    private void OnDestroy()
    {
        CleanUp();
    }
    #endregion

    #region Inicialización
    private void InitializeGame()
    {
        SetupEventListeners();
        ConfigureInitialUI();
        ConfigureAudioSource();
    }

    private void SetupEventListeners()
    {
        leverButton.onClick.AddListener(PullLever);
        startButton.onClick.AddListener(OnDialogueClick);
    }

    private void ConfigureInitialUI()
    {
        rulesPanel.SetActive(true);
        leverButton.gameObject.SetActive(false);
        winPanel.SetActive(false);
        StartTypingDialogue(rulesDialogue[currentDialogueIndex]);
    }

    private void ConfigureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
    #endregion

    #region Manejo de Diálogos
    private void OnDialogueClick()
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
            StartTypingDialogue(rulesDialogue[currentDialogueIndex]);
        }
        else
        {
            StartGame();
        }
    }

    private void StartTypingDialogue(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeDialogueText(text));
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
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }
    #endregion

    #region Lógica del Juego
    private void StartGame()
    {
        rulesPanel.SetActive(false);
        leverButton.gameObject.SetActive(true);
        ResetGame();
    }

    private void ResetGame()
    {
        winPanel.SetActive(false); 
        machineImage.sprite = machineNormal; 
        ResetSlots();
    }

    private void ResetSlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
            {
                slot.sprite = symbols[0];
            }
        }
    }

    private void PullLever()
    {
        if (!isSpinning)
        {
            PlayLeverSound();
            
            if (spinCoroutine != null)
            {
                StopCoroutine(spinCoroutine);
                spinCoroutine = null;
            }
            spinCoroutine = StartCoroutine(SpinSequence());
        }
    }
    #endregion

    #region Corrutinas
    private IEnumerator SpinSequence()
    {
        isSpinning = true;
        machineImage.sprite = machineLeverDown;
        winPanel.SetActive(false); 
        
        var spinSlotsCoroutine = StartCoroutine(SpinSlots());
        yield return new WaitForSeconds(leverDownTime);
        
        machineImage.sprite = machineNormal;
        yield return new WaitForSeconds(totalSpinTime - leverDownTime);
        
        if (spinSlotsCoroutine != null)
            StopCoroutine(spinSlotsCoroutine);
            
        yield return StartCoroutine(ShowResult()); 
        isSpinning = false;
    }

    private IEnumerator SpinSlots()
    {
        float elapsedTime = 0f;
        bool victoriaPreparada = false;
        int simboloGanador = 0;

        bool esTiradaGanadora = Random.Range(0, 100) < probabilidadGanar;

        if (esTiradaGanadora)
        {
            simboloGanador = Random.Range(0, symbols.Length);
            victoriaPreparada = true;
        }

        PlaySpinSound();

        while (elapsedTime < totalSpinTime)
        {
            float progress = elapsedTime / totalSpinTime;
            float currentSpeed = Mathf.Lerp(initialSpinSpeed, finalSpinSpeed, progress);
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null) continue;
                
                int randomSymbol = (progress > 0.85f && victoriaPreparada) ? 
                    simboloGanador : 
                    Random.Range(0, symbols.Length);

                slots[i].sprite = symbols[randomSymbol];
                currentResults[i] = randomSymbol;
            }
            
            elapsedTime += currentSpeed;
            yield return new WaitForSeconds(currentSpeed);
        }
    }

    private IEnumerator ShowResult()
    {
        bool win = currentResults[0] == currentResults[1] && 
                  currentResults[1] == currentResults[2];
        
        machineImage.sprite = win ? machineWin : machineLose;
        
        if (win)
        {
            PlayWinSound();
            winText.text = "Bien, puedes ir al siguiente juego con la vieja";
            winPanel.SetActive(true); 
            
            yield return new WaitForSeconds(winDelay);
            
            SceneManager.LoadScene("MainScene");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.CompleteMinigame("Traga_Monedas");
            }
            PlayerPrefs.SetString("LastScene", "Traga_Monedas");
        }
        else
        {
            if (Random.Range(0, 100) < probabilidadMensajeDerrota)
            {
                winText.text = mensajesDerrota[Random.Range(0, mensajesDerrota.Length)];
                winPanel.SetActive(true);
                
                yield return new WaitForSeconds(resultDisplayTime);
                winPanel.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(resultDisplayTime);
            }
            
            machineImage.sprite = machineNormal;
        }
    }
    #endregion

    #region Manejo de Sonido
    private void PlayLeverSound()
    {
        if (leverPullSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(leverPullSound);
        }
    }

    private void PlaySpinSound()
    {
        if (slotSpinSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(slotSpinSound);
        }
    }

    private void PlayWinSound()
    {
        if (winSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(winSound);
        }
    }
    #endregion

    #region Limpieza
    private void CleanUp()
    {
        if (leverButton != null)
            leverButton.onClick.RemoveListener(PullLever);
            
        if (startButton != null)
            startButton.onClick.RemoveAllListeners();
            
        if (spinCoroutine != null)
            StopCoroutine(spinCoroutine);
            
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
    }
    #endregion
}