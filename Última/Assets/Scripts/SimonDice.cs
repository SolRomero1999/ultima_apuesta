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
    [SerializeField] private Image dealer;
    
    [Header("Configuración de Tiempo")]
    [SerializeField] private float tiempoEntreLlamadas = 0.8f;
    [SerializeField] private float tiempoIluminado = 0.5f;
    [SerializeField] private float tiempoMensaje = 1f;
    
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
    
    private const float ALPHA_APAGADO = 0.2f;
    private const float ALPHA_ENCENDIDO = 1f;

    private int[] secuencia;
    private int nivel = 3;
    private int pasoActual = 0;
    private int currentDialogueIndex = 0;
    
    private bool jugadorTurno = false;
    private bool juegoActivo = false;
    private bool primeraVez = true;
    private bool mostrandoSonidos = false;
    
    private Coroutine mostrarSecuenciaCoroutine;
    private Coroutine mostrarSonidosCoroutine;
    private Coroutine mostrarMensajeCoroutine;
    
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
        ApagarTodosTelefonos();
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
        rulesText.text = primeraVez ? dialogoInicial[0] : dialogoRecordatorio[0];
        rulesPanel.SetActive(true);
        gameOverPanel.SetActive(false);
    }

    private void ApagarTodosTelefonos()
    {
        foreach (Image telefono in telefonos)
        {
            Color colorActual = telefono.color;
            colorActual.a = ALPHA_APAGADO;
            telefono.color = colorActual;
        }
    }
    
    #endregion

    #region Lógica del Juego
    
    private void IniciarJuego()
    {
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

        Color colorOriginal = telefonos[index].color;
        colorOriginal.a = ALPHA_ENCENDIDO;
        telefonos[index].color = colorOriginal;
        
        if (sonidosTelefonos != null && index < sonidosTelefonos.Length && audioSource != null)
        {
            audioSource.PlayOneShot(sonidosTelefonos[index]);
        }
        
        yield return new WaitForSeconds(tiempoIluminado);
        
        colorOriginal.a = ALPHA_APAGADO;
        telefonos[index].color = colorOriginal;
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
        if (mostrandoSonidos) return;

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

    private void ManejarDialogoInicial()
    {
        if (currentDialogueIndex < dialogoInicial.Length - 1)
        {
            rulesText.text = dialogoInicial[currentDialogueIndex];
        }
        else if (currentDialogueIndex == dialogoInicial.Length - 1)
        {
            DetenerCoroutine(ref mostrarSonidosCoroutine);
            mostrarSonidosCoroutine = StartCoroutine(MostrarSonidosTelefonos());
            rulesText.text = dialogoInicial[currentDialogueIndex];
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
            DetenerCoroutine(ref mostrarSonidosCoroutine);
            mostrarSonidosCoroutine = StartCoroutine(MostrarSonidosTelefonos());
            rulesText.text = dialogoRecordatorio[currentDialogueIndex];
        }
        else if (currentDialogueIndex > 1)
        {
            rulesPanel.SetActive(false);
            IniciarJuego();
        }
    }
    
    #endregion

    #region Finalización del Juego
    
    private void GanarJuego()
    {
        juegoActivo = false;
        MostrarMensajeTemporal("¡Bien hecho! Has completado el juego.");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteMinigame("SimonDice");
        }
        PlayerPrefs.SetString("LastScene", "SimonDice");
        
        StartCoroutine(RegresarAMainScene());
    }

    private void PerderJuego()
    {
        juegoActivo = false;
        MostrarMensajeTemporal("Una lástima, te veo pronto");
        StartCoroutine(RegresarAMainScene());
    }

    private IEnumerator RegresarAMainScene()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("MainScene");
    }
    
    #endregion

    #region Utilidades
    
    private void LimpiarCoroutines()
    {
        DetenerCoroutine(ref mostrarSecuenciaCoroutine);
        DetenerCoroutine(ref mostrarSonidosCoroutine);
        DetenerCoroutine(ref mostrarMensajeCoroutine);
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