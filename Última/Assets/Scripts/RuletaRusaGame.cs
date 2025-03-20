using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RuletaRusa : MonoBehaviour
{
    public GameObject rulesPanel; // Panel con las reglas
    public Button startButton;    // Botón "Empecemos"
    public TMP_Text dealerText;   // Texto para los mensajes del dealer
    public GameObject shootPanel; // Panel con los botones de acción del jugador
    public Button dispararDealerButton; // Botón para disparar al dealer
    public Button dispararseButton;     // Botón para dispararse a sí mismo
    public Button girarBarrilButton;    // Botón para girar el barril
    public Image backgroundImage; // Imagen de fondo
    public GameObject blackScreen; // Pantalla negra
    public GameObject dialogueBubble; // Globo de diálogo

    public Sprite imgInicial;         // Imagen del arma en la mesa
    public Sprite imgDealerGira;      // Imagen del dealer girando la recámara
    public Sprite imgDealerDispara;   // Imagen del dealer disparándome
    public Sprite imgDealerSuicida;   // Imagen del dealer disparándose
    public Sprite imgJugadorDispara;  // Imagen del jugador disparándole al dealer
    public Sprite imgJugadorSuicida;  // Imagen del jugador disparándose
    public Sprite imgJugadorGira;     // Imagen del jugador girando la recámara
    public Sprite imgDealerMuerto;    // Imagen del dealer muerto

    private int[] barril = { 1, 2, 3, 4, 5, 6 }; // Posiciones del barril
    private int posicionActual;  // Posición actual del barril
    private int balaReal;        // La posición que tiene la bala real
    private bool esTurnoDelJugador = false; // Controla el turno (inicia con el dealer)
    private bool gameOver = false; // Estado del juego

    void Start()
    {
        // Inicializamos las referencias y ocultamos elementos
        rulesPanel.SetActive(true);
        dealerText.gameObject.SetActive(false);
        shootPanel.SetActive(false);
        backgroundImage.sprite = imgInicial; // Imagen inicial
        blackScreen.SetActive(false); // Desactivar la pantalla negra al inicio
        dialogueBubble.SetActive(false); // Desactivar el globo de diálogo al inicio

        // Asignamos los eventos a los botones
        startButton.onClick.AddListener(StartGame);
        dispararDealerButton.onClick.AddListener(() => StartCoroutine(Disparar(true)));
        dispararseButton.onClick.AddListener(() => StartCoroutine(Disparar(false)));
        girarBarrilButton.onClick.AddListener(() => StartCoroutine(GirarBarril()));
    }

    void StartGame()
    {
        // Inicia el juego, oculta las reglas y comienza la acción
        rulesPanel.SetActive(false);
        dealerText.gameObject.SetActive(true);
        dialogueBubble.SetActive(true); // Activar el globo de diálogo
        dealerText.text = "Si no te molesta, empezaré yo";
        Debug.Log("Mensaje inicial del dealer");
        StartCoroutine(EsperarYEmpezar()); // Espera antes de comenzar las acciones
    }

    IEnumerator EsperarYEmpezar()
    {
        yield return new WaitForSeconds(3f); // Espera 3 segundos para mostrar el mensaje inicial
        PosicionInicial();
        Debug.Log("Inicia el dealer");
        Debug.Log("Posición inicial: " + posicionActual);
        Debug.Log("Bala real en: " + balaReal);
        StartCoroutine(DealerTurn()); // El dealer comienza el juego
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
        backgroundImage.sprite = imgInicial; // Mostrar imagen inicial mientras el dealer elige
        yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice

        int decision = Random.Range(0, 100); 
        Debug.Log("El dealer esta eligiendo..."); 

        if (decision < 50) 
        {
            backgroundImage.sprite = imgDealerDispara; // Cambiar imagen antes del diálogo
            yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
            dealerText.text = "Veamos si la suerte te acompaña...";
            Debug.Log("El dealer eligio dispararte");
            yield return new WaitForSeconds(3f); // Espera 3 segundos
            yield return StartCoroutine(Disparar(true)); // Disparar al jugador
        }
        else if (decision < 80) 
        {
            backgroundImage.sprite = imgDealerSuicida; // Cambiar imagen antes del diálogo
            yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
            dealerText.text = "Espero tener suerte...";
            Debug.Log("El dealer eligio dispararse"); 
            yield return new WaitForSeconds(3f); // Espera 3 segundos
            yield return StartCoroutine(Disparar(false)); // Dispararse a sí mismo
        }
        else 
        {
            backgroundImage.sprite = imgDealerGira; // Cambiar imagen antes del diálogo
            yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
            dealerText.text = "Hagamoslo más interesante";
            Debug.Log("El dealer giro el barril"); 
            yield return new WaitForSeconds(3f); // Espera 3 segundos
            yield return StartCoroutine(GirarBarril());
        }
    }

    IEnumerator GirarBarril()
    {
        posicionActual = (posicionActual + 1) % barril.Length;
        dealerText.text = "Haz que giren.";
        backgroundImage.sprite = esTurnoDelJugador ? imgJugadorGira : imgDealerGira;
        yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
        yield return new WaitForSeconds(3f); // Espera 3 segundos

        if (!esTurnoDelJugador)
        {
            esTurnoDelJugador = true;
            shootPanel.SetActive(true);
            backgroundImage.sprite = imgInicial; // Mostrar imagen inicial mientras el jugador elige
            yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
            Debug.Log("Ahora es el turno del jugador");
        }
        else
        {
            yield return StartCoroutine(DealerTurn());
            Debug.Log("Ahora es el turno del dealer");
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
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("El dealer ha muerto");
                }
                else
                {
                    dealerText.text = "No es tu día...";
                    Debug.Log("Haz muerto");
                    yield return StartCoroutine(PlayerDeath()); // Pantalla negra y transición a Game Over
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    dealerText.text = "No eres muy listo...";
                    backgroundImage.sprite = imgJugadorSuicida; // Mostrar imagen del jugador suicidándose
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("Haz muerto");
                    yield return StartCoroutine(PlayerDeath()); // Pantalla negra y transición a Game Over
                }
                else
                {
                    dealerText.text = "El dealer ha muerto...";
                    backgroundImage.sprite = imgDealerMuerto;
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("El dealer ha muerto");
                }
            }
            gameOver = true;
            yield return new WaitForSeconds(3f); // Espera 3 segundos
            StartCoroutine(EndGame());
        }
        else
        {
            if (dispararAlJugador)
            {
                if (esTurnoDelJugador)
                {
                    dealerText.text = "Ahora es mi turno...";
                    backgroundImage.sprite = imgJugadorDispara;
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("Ahora es el turno del dealer");
                    AvanzarBarril();
                    yield return new WaitForSeconds(3f); // Espera 3 segundos
                    yield return StartCoroutine(DealerTurn());
                }
                else
                {
                    dealerText.text = "Vas con suerte...";
                    backgroundImage.sprite = imgDealerDispara;
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("Ahora es el turno del jugador");
                    AvanzarBarril();
                    esTurnoDelJugador = true;
                    shootPanel.SetActive(true);
                    backgroundImage.sprite = imgInicial; // Mostrar imagen inicial mientras el jugador elige
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                }
            }
            else
            {
                if (esTurnoDelJugador)
                {
                    dealerText.text = "Vas con suerte...";
                    backgroundImage.sprite = imgJugadorSuicida; // Mostrar imagen del jugador suicidándose
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("Ahora es el turno del jugador");
                    AvanzarBarril();
                    esTurnoDelJugador = true;
                    shootPanel.SetActive(true);
                    backgroundImage.sprite = imgInicial; // Mostrar imagen inicial mientras el jugador elige
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                }
                else
                {
                    dealerText.text = "Otra vez mi turno...";
                    backgroundImage.sprite = imgDealerSuicida;
                    yield return new WaitForSeconds(0.5f); // Espera 0.5 segundos para que la imagen se actualice
                    Debug.Log("Ahora es el turno del dealer");
                    AvanzarBarril();
                    yield return new WaitForSeconds(3f); // Espera 3 segundos
                    yield return StartCoroutine(DealerTurn());
                }
            }
        }
    }

    void AvanzarBarril()
    {
        posicionActual = (posicionActual + 1) % barril.Length;
        Debug.Log("Barril avanzado. Nueva posición: " + posicionActual);
    }

    IEnumerator PlayerDeath()
    {
        blackScreen.SetActive(true); // Activar la pantalla negra
        yield return new WaitForSeconds(2f); // Esperar 2 segundos
        SceneManager.LoadScene("GameOver"); // Cargar la escena de Game Over
    }

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(3f);
        SceneManager.LoadScene("MainScene");
    }
}