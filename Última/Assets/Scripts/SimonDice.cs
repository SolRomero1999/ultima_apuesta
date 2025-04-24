using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SimonDice : MonoBehaviour
{
    #region Variables Serializadas
    
    [Header("Referencias Visuales")]
    [SerializeField] private Image[] telefonos;
    [SerializeField] private Animator[] telefonosAnimators; // Nuevo: Animators para cada teléfono
    [SerializeField] private Image dealer;
    [SerializeField] private Sprite dealerReglas;
    [SerializeField] private Sprite dealerJuego;
    [SerializeField] private Sprite dealerLose;
    [SerializeField] private GameObject blackScreen;
    
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEntreLlamadas = 0.8f;
    [SerializeField] private float tiempoIluminado = 0.5f;
    [SerializeField] private float tiempoMensaje = 1f;
    [SerializeField] private float textSpeed = 0.05f;
    
    [Header("Configuración de Sonido")]
    [SerializeField] private AudioClip sonidoDealer;
    [SerializeField] private AudioClip[] sonidosTelefonos;
    [SerializeField] private AudioSource audioSource;
    
    [Header("UI Elements")]
    [SerializeField] private GameObject rulesPanel;
    [SerializeField] private TMP_Text rulesText;
    [SerializeField] private Button continueButton;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text gameOverText;
    
    #endregion

    #region Variables Privadas
    
    private int[] secuencia;
    private int nivel = 3;
    private int pasoActual = 0;
    private int currentDialogueIndex = 0;
    
    private bool jugadorTurno = false;
    private bool juegoActivo = false;
    private bool primeraVez = true;
    private bool mostrandoSonidos = false;
    private bool isTyping = false;
    private bool skipTyping = false;
    
    private Coroutine mostrarSecuenciaCoroutine;
    private Coroutine mostrarSonidosCoroutine;
    private Coroutine mostrarMensajeCoroutine;
    private Coroutine typingCoroutine;
    
    private readonly string[] dialogoInicial = {
        "Al fin es mi turno de jugar contigo",
        "Te explicare lo que haremos",
        "Jugaremos 5 rondas de simón dice",
        "En la primera ronda sonaran 3 teléfonos en cierto orden",
        "Luego de que terminen de sonar debes atenderlos en el orden que sonaron",
        "Te mostrare como suena cada uno antes de empezar",
        "¿Te haz enterado? Empecemos"
    };

    private readonly string[] dialogoRecordatorio = {
        "Te refrescare la memoria, así suena cada teléfono",
        "Bien, ahora empecemos"
    };

    private readonly string[] mensajesVictoria = {
        "Bien hecho, ya estas listo para el final",
        "Impresionante, ve a ver al Bartender",
    };

    private readonly string[] mensajesDerrota = {
        "Una lástima, te veo pronto",
        "No fue suficiente esta vez",
        "Mejor suerte para la próxima"
    };
    
    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        continueButton.onClick.AddListener(AdvanceDialogue);
    }

    private void Start()
    {
        primeraVez = PlayerPrefs.GetInt("SimonDice_PlayedBefore", 0) == 0;
        PlayerPrefs.SetInt("SimonDice_PlayedBefore", 1);
        
        InicializarUI();
        dealer.sprite = dealerReglas;
        blackScreen.SetActive(false);
    }

    private void OnDestroy()
    {
        LimpiarCoroutines();
        continueButton.onClick.RemoveListener(AdvanceDialogue);
    }
    
    #endregion

    #region Inicialización
    
    private void InicializarUI()
    {
        rulesText.text = "";
        rulesPanel.SetActive(true);
        gameOverPanel.SetActive(false);
        
        if (primeraVez)
        {
            StartCoroutine(TypeDialogueText(dialogoInicial[0]));
        }
        else
        {
            StartCoroutine(TypeDialogueText(dialogoRecordatorio[0]));
        }
    }
    
    #endregion

    #region Lógica del Juego
    
    private void IniciarJuego()
    {
        dealer.sprite = dealerJuego;
        juegoActivo = true;
        nivel = 3;
        GenerarSecuencia();
        
        DetenerCoroutine(ref mostrarSecuenciaCoroutine);
        mostrarSecuenciaCoroutine = StartCoroutine(MostrarSecuencia());
    }

    private void GenerarSecuencia()
    {
        secuencia = new int[nivel];
        for (int i = 0; i < nivel; i++)
        {
            secuencia[i] = Random.Range(0, telefonos.Length);
        }
    }

    private IEnumerator MostrarSecuencia()
    {
        jugadorTurno = false;
        
        yield return new WaitForSeconds(1f);
        
        if (sonidoDealer != null && audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(sonidoDealer);
            yield return new WaitForSeconds(0.3f);
        }
        
        for (int i = 0; i < nivel; i++)
        {
            int telefonoIndex = secuencia[i];
            yield return StartCoroutine(IluminarTelefono(telefonoIndex));
            yield return new WaitForSeconds(tiempoEntreLlamadas);
        }
        
        MostrarMensajeTemporal("¡Ahora imita la secuencia!");
        
        jugadorTurno = true;
        pasoActual = 0;
    }

    private IEnumerator IluminarTelefono(int index)
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Activar animación de ring
        if (index < telefonosAnimators.Length && telefonosAnimators[index] != null)
        {
            string triggerName = "Ring" + (index + 1).ToString();
            telefonosAnimators[index].SetTrigger(triggerName);
        }
        
        if (sonidosTelefonos != null && index < sonidosTelefonos.Length && audioSource != null)
        {
            audioSource.PlayOneShot(sonidosTelefonos[index]);
        }
        
        yield return new WaitForSeconds(tiempoIluminado);
    }

    public void TelefonoClicado(int telefonoIndex)
    {
        if (!jugadorTurno || !juegoActivo || mostrandoSonidos) return;
        
        StartCoroutine(IluminarTelefono(telefonoIndex));
        VerificarJugada(telefonoIndex);
    }

    private void VerificarJugada(int telefonoIndex)
    {
        if (secuencia[pasoActual] == telefonoIndex)
        {
            pasoActual++;
            
            if (pasoActual >= nivel)
            {
                if (nivel >= 7)
                {
                    GanarJuego();
                }
                else
                {
                    AvanzarNivel();
                }
            }
        }
        else
        {
            PerderJuego();
        }
    }

    private void AvanzarNivel()
    {
        nivel++;
        jugadorTurno = false;
        pasoActual = 0;
        GenerarSecuencia();
        
        DetenerCoroutine(ref mostrarSecuenciaCoroutine);
        mostrarSecuenciaCoroutine = StartCoroutine(MostrarSecuencia());
    }
    
    #endregion

    #region Finalización del Juego
    
    private void GanarJuego()
    {
        juegoActivo = false;
        string mensaje = mensajesVictoria[Random.Range(0, mensajesVictoria.Length)];
        MostrarMensajeTemporal(mensaje);
        
        StartCoroutine(GanarJuegoCoroutine());
    }

    private IEnumerator GanarJuegoCoroutine()
    {
        yield return new WaitForSeconds(2f);
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteMinigame("Simon_Dice");
        }
        if (AnomaliasController.Instance != null)
        {
            AnomaliasController.Instance.RemoveAnomalia();
        }
        PlayerPrefs.SetString("LastScene", "Simon_Dice");
        SceneManager.LoadScene("MainScene");
    }

    private void PerderJuego()
    {
        juegoActivo = false;
        dealer.sprite = dealerLose;
        string mensaje = mensajesDerrota[Random.Range(0, mensajesDerrota.Length)];
        MostrarMensajeTemporal(mensaje);
        
        StartCoroutine(PerderJuegoCoroutine());
    }

    private IEnumerator PerderJuegoCoroutine()
    {
        yield return new WaitForSeconds(2f);
        blackScreen.SetActive(true);
        yield return new WaitForSeconds(1f);
        
        if (AnomaliasController.Instance != null)
        {
            AnomaliasController.Instance.AddAnomalia();
        }
        PlayerPrefs.SetString("LastScene", "Simon_Dice");
        SceneManager.LoadScene("MainScene");
    }
    
    #endregion

    #region Manejo de UI
    
    private void MostrarMensajeTemporal(string mensaje)
    {
        DetenerCoroutine(ref mostrarMensajeCoroutine);
        mostrarMensajeCoroutine = StartCoroutine(MostrarMensajeTemporalCoroutine(mensaje));
    }

    private IEnumerator MostrarMensajeTemporalCoroutine(string mensaje)
    {
        gameOverText.text = mensaje;
        gameOverPanel.SetActive(true);
        yield return new WaitForSeconds(tiempoMensaje);
        gameOverPanel.SetActive(false);
        mostrarMensajeCoroutine = null;
    }

    private IEnumerator MostrarSonidosTelefonos()
    {
        mostrandoSonidos = true;
        rulesPanel.SetActive(false);
        
        yield return new WaitForSeconds(0.5f);
        
        for (int i = 0; i < telefonos.Length; i++)
        {
            yield return StartCoroutine(IluminarTelefono(i));
            yield return new WaitForSeconds(tiempoEntreLlamadas);
        }
        
        rulesPanel.SetActive(true);
        mostrandoSonidos = false;
    }

    private void AdvanceDialogue()
    {
        if (isTyping && !skipTyping)
        {
            skipTyping = true;
            return;
        }

        skipTyping = false;
        currentDialogueIndex++;
        
        if (primeraVez)
        {
            ManejarDialogoInicial();
        }
        else
        {
            ManejarDialogoRecordatorio();
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
            yield return new WaitForSeconds(textSpeed);
        }
        
        isTyping = false;
    }

    private void ManejarDialogoInicial()
    {
        if (currentDialogueIndex < dialogoInicial.Length - 1)
        {
            StartCoroutine(TypeDialogueText(dialogoInicial[currentDialogueIndex]));
        }
        else if (currentDialogueIndex == dialogoInicial.Length - 1)
        {
            StartCoroutine(TypeDialogueText(dialogoInicial[currentDialogueIndex]));
            DetenerCoroutine(ref mostrarSonidosCoroutine);
            mostrarSonidosCoroutine = StartCoroutine(MostrarSonidosTelefonos());
        }
        else
        {
            rulesPanel.SetActive(false);
            IniciarJuego();
        }
    }

    private void ManejarDialogoRecordatorio()
    {
        if (currentDialogueIndex == 1)
        {
            StartCoroutine(TypeDialogueText(dialogoRecordatorio[currentDialogueIndex]));
            DetenerCoroutine(ref mostrarSonidosCoroutine);
            mostrarSonidosCoroutine = StartCoroutine(MostrarSonidosTelefonos());
        }
        else if (currentDialogueIndex > 1)
        {
            rulesPanel.SetActive(false);
            IniciarJuego();
        }
    }
    
    #endregion

    #region Utilidades
    
    private void LimpiarCoroutines()
    {
        DetenerCoroutine(ref mostrarSecuenciaCoroutine);
        DetenerCoroutine(ref mostrarSonidosCoroutine);
        DetenerCoroutine(ref mostrarMensajeCoroutine);
        DetenerCoroutine(ref typingCoroutine);
    }

    private void DetenerCoroutine(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
    
    #endregion
}