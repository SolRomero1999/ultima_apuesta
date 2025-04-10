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
    public TMP_Text playerPairsText;
    public TMP_Text dealerPairsText;
    #endregion

    #region Diálogos
    private string[] introDialogue = {
        "En otro tiempo te hubiera dicho tu fortuna",
        "Pero viendo que es algo obvio solo jugaremos algo simple",
        "Un juego de memoria, con cartas",
        "Básicamente nos iremos turnando para voltear dos cartas",
        "Si formas una pareja te quedas esas cartas y vuelves a jugar",
        "Si no lo son vuelves a voltearlas y pasa el turno",
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
    private int[] memoriaDealer = new int[6]; 
    private int indiceMemoria = 0;
    private int[] cartasJugadorTurnoAnterior = new int[2] { -1, -1 };
    private bool seguirMismoTurno = false;
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
        for (int i = 0; i < memoriaDealer.Length; i++)
        {
            memoriaDealer[i] = -1;
        }
        indiceMemoria = 0;

        gamePanel.SetActive(false);
        turnPanel.SetActive(false);
        blackScreen.SetActive(false);
        rulesPanel.SetActive(true);
        rulesText.text = introDialogue[currentDialogueIndex];
        nextButton.onClick.AddListener(AdvanceDialogue);
        
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
        UpdatePairsUI();
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

        int primeraCartaIndex = BuscarParejaEnMemoria();
        if (primeraCartaIndex == -1)
        {
            primeraCartaIndex = SeleccionarCartaAleatoria(excluirCartasJugador: true);
        }
        
        yield return StartCoroutine(VoltearCartaAnimacion(primeraCartaIndex));
        AgregarAMemoria(primeraCartaIndex);
        yield return new WaitForSeconds(0.5f);

        int segundaCartaIndex = BuscarParejaEnMemoria(primeraCartaIndex);
        if (segundaCartaIndex == -1)
        {
            segundaCartaIndex = SeleccionarCartaAleatoria(primeraCartaIndex, excluirCartasJugador: true);
        }
        
        yield return StartCoroutine(VoltearCartaAnimacion(segundaCartaIndex));
        AgregarAMemoria(segundaCartaIndex);
        yield return new WaitForSeconds(0.5f);

        if (cartas[primeraCartaIndex].Diseno == cartas[segundaCartaIndex].Diseno)
        {
            paresDealer++;
            UpdatePairsUI();
            cartas[primeraCartaIndex].Emparejar();
            cartas[segundaCartaIndex].Emparejar();
            turnText.text = "¡Pareja encontrada!";
            LimpiarMemoria();

            if (QuedanParesPorEncontrar())
            {
                seguirMismoTurno = true;
                StartCoroutine(DealerTurn());
            }
            else
            {
                FinalizarJuego();
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(VoltearCartaAnimacion(primeraCartaIndex, false));
            yield return StartCoroutine(VoltearCartaAnimacion(segundaCartaIndex, false));
            turnText.text = "No fue pareja...";
            
            if (QuedanParesPorEncontrar())
            {
                seguirMismoTurno = false;
                StartCoroutine(PlayerTurn());
            }
            else
            {
                FinalizarJuego();
            }
        }
    }

    private void AgregarAMemoria(int cartaID)
    {
        if (cartas[cartaID].EstaEmparejada) return;
        
        memoriaDealer[indiceMemoria] = cartaID;
        indiceMemoria = (indiceMemoria + 1) % memoriaDealer.Length;
    }

    private int BuscarParejaEnMemoria(int cartaID = -1)
    {
        if (cartaID == -1)
        {
            for (int i = 0; i < memoriaDealer.Length; i++)
            {
                int id1 = memoriaDealer[i];
                if (id1 == -1 || cartas[id1].EstaEmparejada || cartasVolteadas[id1]) continue;
                
                for (int j = i + 1; j < memoriaDealer.Length; j++)
                {
                    int id2 = memoriaDealer[j];
                    if (id2 != -1 && !cartas[id2].EstaEmparejada && !cartasVolteadas[id2] && 
                        cartas[id1].Diseno == cartas[id2].Diseno)
                    {
                        return id1;
                    }
                }
            }
        }
        else
        {
            Sprite diseñoBuscado = cartas[cartaID].Diseno;
            for (int i = 0; i < memoriaDealer.Length; i++)
            {
                int id = memoriaDealer[i];
                if (id != -1 && id != cartaID && !cartas[id].EstaEmparejada && 
                    !cartasVolteadas[id] && cartas[id].Diseno == diseñoBuscado)
                {
                    return id;
                }
            }
        }
        
        return -1;
    }

    private void LimpiarMemoria()
    {
        for (int i = 0; i < memoriaDealer.Length; i++)
        {
            if (memoriaDealer[i] != -1 && cartas[memoriaDealer[i]].EstaEmparejada)
            {
                memoriaDealer[i] = -1;
            }
        }
    }

    private IEnumerator PlayerTurn()
    {
        if (!seguirMismoTurno)
        {
            cartasJugadorTurnoAnterior[0] = -1;
            cartasJugadorTurnoAnterior[1] = -1;
        }

        esTurnoJugador = true;
        puedeSeleccionar = true;
        primeraCartaSeleccionada = null;
        segundaCartaSeleccionada = null;
        
        turnText.text = seguirMismoTurno ? "¡Encontraste una pareja! Sigue jugando..." : "Tu turno";
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
        AgregarAMemoria(id);

        if (primeraCartaSeleccionada == null)
        {
            primeraCartaSeleccionada = cartaSeleccionada;
            cartasJugadorTurnoAnterior[0] = id;
        }
        else
        {
            segundaCartaSeleccionada = cartaSeleccionada;
            cartasJugadorTurnoAnterior[1] = id;
            puedeSeleccionar = false;
            StartCoroutine(VerificarParejaJugador());
        }
    }

    private IEnumerator VerificarParejaJugador()
    {
        if (primeraCartaSeleccionada.Diseno == segundaCartaSeleccionada.Diseno)
        {
            paresJugador++;
            UpdatePairsUI();
            primeraCartaSeleccionada.Emparejar();
            segundaCartaSeleccionada.Emparejar();
            turnText.text = "¡Pareja encontrada!";
            LimpiarMemoria();
            
            if (QuedanParesPorEncontrar())
            {
                seguirMismoTurno = true;
                StartCoroutine(PlayerTurn());
            }
            else
            {
                FinalizarJuego();
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);
            cartasVolteadas[System.Array.IndexOf(cartas, primeraCartaSeleccionada)] = false;
            cartasVolteadas[System.Array.IndexOf(cartas, segundaCartaSeleccionada)] = false;
            primeraCartaSeleccionada.Voltear();
            segundaCartaSeleccionada.Voltear();
            turnText.text = "No fue pareja...";
            
            if (QuedanParesPorEncontrar())
            {
                seguirMismoTurno = false;
                StartCoroutine(DealerTurn());
            }
            else
            {
                FinalizarJuego();
            }
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

    private int SeleccionarCartaAleatoria(int excluir = -1, bool excluirCartasJugador = false)
    {
        var indicesDisponibles = Enumerable.Range(0, cartas.Length)
            .Where(i => !cartas[i].EstaEmparejada && !cartasVolteadas[i] && i != excluir)
            .ToList();

        if (excluirCartasJugador)
        {
            indicesDisponibles.RemoveAll(i => i == cartasJugadorTurnoAnterior[0] || i == cartasJugadorTurnoAnterior[1]);
        }

        if (indicesDisponibles.Count == 0)
        {
            indicesDisponibles = Enumerable.Range(0, cartas.Length)
                .Where(i => !cartas[i].EstaEmparejada && !cartasVolteadas[i] && i != excluir)
                .ToList();
        }

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
        yield return new WaitForSeconds(2f);
        blackScreen.SetActive(true);
        PlayerPrefs.SetString("LastScene", "Juego_Memoria");
        SceneManager.LoadScene("MainScene");
    }
    #endregion
}