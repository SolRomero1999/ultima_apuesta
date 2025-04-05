using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Linq;

public class JuegoMemoria : MonoBehaviour
{
    #region Configuración UI
    [Header("Configuración UI")]
    public GameObject rulesPanel;
    public Button nextButton;
    public TMP_Text rulesText;
    public Image tarotistaImage;
    public GameObject gamePanel;
    public GameObject cartaPrefab;
    public Transform gridLayout;
    public Sprite[] diseñosCartas;
    public Sprite reversoCarta;
    public GameObject turnPanel;
    public TMP_Text turnText;
    public GameObject blackScreen;
    public TMP_Text playerPairsText; // Texto para parejas del jugador
    public TMP_Text dealerPairsText; // Texto para parejas del dealer
    #endregion

    #region Diálogos
    private string[] introDialogue = {
        "En otro tiempo te hubiera dicho tu fortuna",
        "Pero viendo que es algo obvio solo jugaremos algo simple",
        "Un juego de memoria, con cartas",
        "Básicamente nos iremos turnando para voltear dos cartas",
        "Si formas una pareja te quedas esas cartas",
        "Si no lo son vuelves a voltearlas",
        "Al final gana quien tenga más parejas"
    };
    
    private string[] dealerWinMessages = {
        "Mala fortuna para ti",
        "El destino no está de tu lado",
        "Las cartas no mienten... has perdido"
    };
    
    private string[] playerWinMessages = {
        "Te depara un gran futuro",
        "La suerte está de tu lado... por ahora",
        "Has vencido esta vez"
    };
    #endregion

    #region Variables del Juego
    private int currentDialogueIndex = 0;
    private Carta[] cartas;
    private Carta primeraCartaSeleccionada;
    private Carta segundaCartaSeleccionada;
    private bool puedeSeleccionar = false;
    private int paresJugador = 0;
    private int paresDealer = 0;
    private bool esTurnoJugador = false;
    private bool[] cartasVolteadas;
    private bool primerTurnoDealer = true;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        InitializeGame();
    }
    #endregion

    #region Inicialización
    private void InitializeGame()
    {
        gamePanel.SetActive(false);
        turnPanel.SetActive(false);
        blackScreen.SetActive(false);
        rulesPanel.SetActive(true);
        rulesText.text = introDialogue[currentDialogueIndex];
        nextButton.onClick.AddListener(AdvanceDialogue);
        
        // Inicializar textos (aunque no son visibles todavía)
        playerPairsText.text = "Tus parejas: 0";
        dealerPairsText.text = "Mis parejas: 0";
    }
    #endregion

    #region Manejo de Diálogos
    private void AdvanceDialogue()
    {
        currentDialogueIndex++;
        
        if (currentDialogueIndex < introDialogue.Length)
        {
            rulesText.text = introDialogue[currentDialogueIndex];
        }
        else
        {
            StartGame();
        }
    }
    #endregion

    #region Lógica del Juego
    private void StartGame()
    {
        rulesPanel.SetActive(false);
        tarotistaImage.gameObject.SetActive(false);
        gamePanel.SetActive(true);
        CrearTablero();
        UpdatePairsUI(); // Actualizar UI al iniciar
        StartCoroutine(DealerTurn());
    }

    private void CrearTablero()
    {
        foreach (Transform child in gridLayout)
        {
            Destroy(child.gameObject);
        }

        Sprite[] diseñosParaAsignar = new Sprite[18];
        for (int i = 0; i < diseñosCartas.Length; i++)
        {
            diseñosParaAsignar[i * 2] = diseñosCartas[i];
            diseñosParaAsignar[i * 2 + 1] = diseñosCartas[i];
        }

        System.Random rnd = new System.Random();
        diseñosParaAsignar = diseñosParaAsignar.OrderBy(x => rnd.Next()).ToArray();

        cartas = new Carta[18];
        cartasVolteadas = new bool[18];
        
        for (int i = 0; i < 18; i++)
        {
            GameObject nuevaCarta = Instantiate(cartaPrefab, gridLayout);
            Carta cartaComponent = nuevaCarta.GetComponent<Carta>();
            cartaComponent.Inicializar(diseñosParaAsignar[i], reversoCarta, i);
            cartaComponent.OnCartaClickeada += ManejarClicCarta;
            cartas[i] = cartaComponent;
            cartasVolteadas[i] = false;
        }
    }

    // Método para actualizar los textos de las parejas
    private void UpdatePairsUI()
    {
        playerPairsText.text = $"Tus parejas: {paresJugador}";
        dealerPairsText.text = $"Mis parejas: {paresDealer}";
    }

    private IEnumerator DealerTurn()
    {
        turnPanel.SetActive(true);
        
        if (primerTurnoDealer)
        {
            turnText.text = "Daré inicio...";
            primerTurnoDealer = false;
        }
        else
        {
            turnText.text = "Mi turno...";
        }
        
        yield return new WaitForSeconds(1.5f);

        esTurnoJugador = false;
        yield return new WaitForSeconds(0.5f);

        int primeraCartaIndex = SeleccionarCartaAleatoria();
        yield return StartCoroutine(VoltearCartaAnimacion(primeraCartaIndex));
        yield return new WaitForSeconds(0.5f);

        int segundaCartaIndex = SeleccionarCartaAleatoria(primeraCartaIndex);
        yield return StartCoroutine(VoltearCartaAnimacion(segundaCartaIndex));
        yield return new WaitForSeconds(0.5f);

        if (cartas[primeraCartaIndex].Diseno == cartas[segundaCartaIndex].Diseno)
        {
            paresDealer++;
            UpdatePairsUI(); // Actualizar UI cuando el dealer consigue pareja
            cartas[primeraCartaIndex].Emparejar();
            cartas[segundaCartaIndex].Emparejar();
            turnText.text = "¡Pareja encontrada!";
        }
        else
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(VoltearCartaAnimacion(primeraCartaIndex, false));
            yield return StartCoroutine(VoltearCartaAnimacion(segundaCartaIndex, false));
            turnText.text = "No fue pareja...";
        }

        yield return new WaitForSeconds(1f);

        if (QuedanParesPorEncontrar())
        {
            StartCoroutine(PlayerTurn());
        }
        else
        {
            FinalizarJuego();
        }
    }

    private IEnumerator PlayerTurn()
    {
        esTurnoJugador = true;
        puedeSeleccionar = true;
        primeraCartaSeleccionada = null;
        segundaCartaSeleccionada = null;
        
        turnText.text = "Tu turno";
        yield return new WaitForSeconds(0.5f);
    }

    private void ManejarClicCarta(int id)
    {
        if (!puedeSeleccionar || !esTurnoJugador || cartasVolteadas[id]) return;
        
        Carta cartaSeleccionada = cartas[id];
        
        if (cartaSeleccionada.EstaVolteada || cartaSeleccionada.EstaEmparejada) 
            return;

        cartaSeleccionada.Voltear();
        cartasVolteadas[id] = true;

        if (primeraCartaSeleccionada == null)
        {
            primeraCartaSeleccionada = cartaSeleccionada;
        }
        else
        {
            segundaCartaSeleccionada = cartaSeleccionada;
            puedeSeleccionar = false;
            StartCoroutine(VerificarParejaJugador());
        }
    }

    private IEnumerator VerificarParejaJugador()
    {
        if (primeraCartaSeleccionada.Diseno == segundaCartaSeleccionada.Diseno)
        {
            paresJugador++;
            UpdatePairsUI(); // Actualizar UI cuando el jugador consigue pareja
            primeraCartaSeleccionada.Emparejar();
            segundaCartaSeleccionada.Emparejar();
            turnText.text = "Tienes suerte";
        }
        else
        {
            yield return new WaitForSeconds(1f);
            cartasVolteadas[System.Array.IndexOf(cartas, primeraCartaSeleccionada)] = false;
            cartasVolteadas[System.Array.IndexOf(cartas, segundaCartaSeleccionada)] = false;
            primeraCartaSeleccionada.Voltear();
            segundaCartaSeleccionada.Voltear();
            turnText.text = "Quizas la próxima...";
        }

        yield return new WaitForSeconds(1f);

        if (QuedanParesPorEncontrar())
        {
            StartCoroutine(DealerTurn());
        }
        else
        {
            FinalizarJuego();
        }
    }

    private IEnumerator VoltearCartaAnimacion(int id, bool voltear = true)
    {
        if (voltear)
        {
            cartas[id].Voltear();
            cartasVolteadas[id] = true;
        }
        else
        {
            cartas[id].Voltear();
            cartasVolteadas[id] = false;
        }
        yield return new WaitForSeconds(0.5f);
    }

    private int SeleccionarCartaAleatoria(int excluir = -1)
    {
        var indicesDisponibles = Enumerable.Range(0, cartas.Length)
            .Where(i => !cartas[i].EstaEmparejada && !cartasVolteadas[i] && i != excluir)
            .ToList();

        if (indicesDisponibles.Count == 0) return -1;

        return indicesDisponibles[Random.Range(0, indicesDisponibles.Count)];
    }

    private bool QuedanParesPorEncontrar()
    {
        return cartas.Count(c => !c.EstaEmparejada) > 0;
    }

    private void FinalizarJuego()
    {
        if (paresJugador > paresDealer)
        {
            string mensaje = playerWinMessages[Random.Range(0, playerWinMessages.Length)];
            turnText.text = mensaje;
            StartCoroutine(PlayerWon());
        }
        else
        {
            string mensaje = dealerWinMessages[Random.Range(0, dealerWinMessages.Length)];
            turnText.text = mensaje;
            StartCoroutine(PlayerDeath());
        }
    }

    private IEnumerator PlayerWon()
    {
        yield return new WaitForSeconds(2f);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteMinigame("Juego_Memoria");
        }
        PlayerPrefs.SetString("LastScene", "Juego_Memoria");
        SceneManager.LoadScene("MainScene");
    }

    private IEnumerator PlayerDeath()
    {
        blackScreen.SetActive(true);
        yield return new WaitForSeconds(2f);
        PlayerPrefs.SetString("LastScene", "Juego_Memoria");
        SceneManager.LoadScene("MainScene");
    }
    #endregion
}