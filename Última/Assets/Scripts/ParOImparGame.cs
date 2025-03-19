using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ParOImparGame : MonoBehaviour
{
    public GameObject rulesPanel; // Panel con las reglas
    public Button startButton;    // Botón "Empecemos"
    public TMP_Text dealerText;   // Texto para los mensajes del dealer
    public TMP_Text playerTeethText;    // Texto que muestra cuántos dientes tiene el jugador
    public TMP_Text dealerTeethText;    // Texto que muestra cuántos dientes tiene el dealer
    public Button parButton;      // Botón para elegir "Par"
    public Button imparButton;    // Botón para elegir "Impar"
    public GameObject betPanel;   // Panel de apuesta del jugador
    public TMP_Text betAmountText; // Texto que muestra la cantidad apostada
    public Button increaseBetButton; // Botón para incrementar la apuesta
    public Button decreaseBetButton; // Botón para disminuir la apuesta
    public Button betButton;      // Botón para apostar
    public Image toothImage;      // Imagen del diente al lado del número

    private int playerTeeth = 10;   // Cantidad inicial de dientes del jugador
    private int dealerTeeth = 10;   // Cantidad inicial de dientes del dealer
    private int dealerNumber;       // Número elegido por el dealer
    private int betAmount = 1;      // Dientes apostados por el jugador
    private bool gameOver = false;  // Estado del juego
    private bool isFirstTurn = true; // Controla si es la primera vez que el dealer juega

    void Start()
    {
        // Mostrar reglas y ocultar el resto de la interfaz al inicio
        rulesPanel.SetActive(true);
        dealerText.gameObject.SetActive(false);
        playerTeethText.gameObject.SetActive(false);
        dealerTeethText.gameObject.SetActive(false);
        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
        betPanel.SetActive(false);

        // Mostrar dientes iniciales
        playerTeethText.text = "Dientes del jugador: " + playerTeeth;
        dealerTeethText.text = "Dientes del dealer: " + dealerTeeth;

        // Asignar función al botón de inicio
        startButton.onClick.AddListener(StartGame);

        // Asignar funciones a los botones de apuesta
        increaseBetButton.onClick.AddListener(IncreaseBet);
        decreaseBetButton.onClick.AddListener(DecreaseBet);
        betButton.onClick.AddListener(() => StartCoroutine(MakeBetCoroutine())); // Convertir en corrutina
    }

    void StartGame()
    {
        rulesPanel.SetActive(false); // Ocultar reglas

        // Activar elementos del juego
        dealerText.gameObject.SetActive(true);
        playerTeethText.gameObject.SetActive(true);
        dealerTeethText.gameObject.SetActive(true);
        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
        betPanel.SetActive(false);   // Mostrar panel de apuesta

        // Iniciar diálogo del dealer
        StartCoroutine(DealerTurn());
    }

    IEnumerator DealerTurn()
    {
        if (gameOver) yield break; // Detener si el juego ha terminado

        betPanel.SetActive(false);
        increaseBetButton.gameObject.SetActive(false);
        decreaseBetButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);

        // Mostrar mensaje adecuado según si es el primer turno
        if (isFirstTurn)
        {
            dealerText.text = "El dealer empieza. Elegirá un número...";
            isFirstTurn = false;  // Cambiar a false después de la primera vez
        }
        else
        {
            dealerText.text = "Es el turno del dealer.";
        }

        yield return new WaitForSeconds(2f); // Pausa para leer el mensaje

        // El dealer elige un número entre 1 y el mínimo entre sus dientes y 10
        dealerNumber = Random.Range(1, Mathf.Min(dealerTeeth, playerTeeth, 10) + 1);
        Debug.Log("Número elegido por el dealer: " + dealerNumber); // Mostrar en consola

        dealerText.text = "Ok, ya elegí un número. ¿Crees que es Par o Impar?";
        yield return new WaitForSeconds(2f); // Pausa para leer el mensaje

        // Activar botones para que el jugador elija
        parButton.gameObject.SetActive(true);
        imparButton.gameObject.SetActive(true);

        // Asignar funciones a los botones
        parButton.onClick.RemoveAllListeners();
        imparButton.onClick.RemoveAllListeners();
        parButton.onClick.AddListener(() => StartCoroutine(PlayerChoiceCoroutine(true)));
        imparButton.onClick.AddListener(() => StartCoroutine(PlayerChoiceCoroutine(false)));
    }

    IEnumerator PlayerChoiceCoroutine(bool chosePar)
    {
        if (gameOver) yield break; // Detener si el juego ha terminado

        // Determinar si el número del dealer es par o impar
        bool isDealerNumberPar = (dealerNumber % 2 == 0);

        if (chosePar == isDealerNumberPar)
        {
            dealerText.text = "¡Bien hecho! Elegiste correctamente.";
            playerTeeth += dealerNumber; // Sumar los dientes ganados
            dealerTeeth -= dealerNumber; // Restar los dientes del dealer
        }
        else
        {
            dealerText.text = "¡Ups! Fallaste.";
            playerTeeth -= dealerNumber; // Restar los dientes perdidos
            dealerTeeth += dealerNumber; // Sumar los dientes al dealer
        }

        // Actualizar los textos de dientes del jugador y del dealer por separado
        playerTeethText.text = "Dientes del jugador: " + playerTeeth;
        dealerTeethText.text = "Dientes del dealer: " + dealerTeeth;

        // Esperar 2 segundos para que el jugador pueda leer el mensaje
        yield return new WaitForSeconds(2f);

        // Verificar condiciones de victoria o derrota
        if (playerTeeth >= 20)
        {
            dealerText.text = "¡Ganaste! ¡Tienes 20 dientes!";
            gameOver = true;
            StartCoroutine(EndGame());  // Cargar la escena principal después de ganar
        }
        else if (playerTeeth <= 0)
        {
            dealerText.text = "¡Perdiste! Te quedaste sin dientes...";
            gameOver = true;
            StartCoroutine(EndGame());  // Cargar la escena principal después de perder
        }
        else if (dealerTeeth <= 0)
        {
            dealerText.text = "¡El dealer se quedó sin dientes!";
            gameOver = true;
            StartCoroutine(EndGame());  // Cargar la escena principal después de que el dealer pierda
        }
        else
        {
            // Después del turno del jugador, cambiar turno
            StartCoroutine(PlayerTurn());
        }

        // Desactivar botones para evitar múltiples clics
        parButton.gameObject.SetActive(false);
        imparButton.gameObject.SetActive(false);
    }

    // Función para manejar el turno del jugador
    IEnumerator PlayerTurn()
    {
        if (gameOver) yield break;

        dealerText.text = "Es tu turno. Elige cuántos dientes apostar.";
        yield return new WaitForSeconds(2f); // Pausa para leer el mensaje

        betPanel.SetActive(true);   // Mostrar panel de apuesta

        betAmount = 1; // Iniciar apuesta con 1
        betAmountText.text = betAmount.ToString();

        // Activar los botones de apuesta
        increaseBetButton.gameObject.SetActive(true);
        decreaseBetButton.gameObject.SetActive(true);
        betButton.gameObject.SetActive(true);
    }

    // Aumentar la apuesta
    void IncreaseBet()
    {
        if (betAmount < Mathf.Min(10, playerTeeth, dealerTeeth)) // No puede apostar más dientes de los que tiene o más de 10
        {
            betAmount++;
            betAmountText.text = betAmount.ToString();
        }
    }

    // Disminuir la apuesta
    void DecreaseBet()
    {
        if (betAmount > 1) // No puede apostar menos de 1
        {
            betAmount--;
            betAmountText.text = betAmount.ToString();
        }
    }

    // Corrutina para manejar la apuesta
    IEnumerator MakeBetCoroutine()
    {
        if (gameOver) yield break;

        // Desactivar botones de apuesta
        increaseBetButton.gameObject.SetActive(false);
        decreaseBetButton.gameObject.SetActive(false);
        betButton.gameObject.SetActive(false);

        // El dealer adivina si el número apostado es par o impar
        bool dealerGuess = Random.Range(0, 2) == 0; // Adivina al azar (puedes mejorarlo con lógica más compleja)

        dealerText.text = "El dealer cree que el número es " + (dealerGuess ? "Par" : "Impar");
        yield return new WaitForSeconds(2f); // Pausa para leer el mensaje

        // Hacer una pequeña pausa para que el jugador vea la apuesta del dealer
        yield return StartCoroutine(WaitAndShowResult(dealerGuess));
    }

    IEnumerator WaitAndShowResult(bool dealerGuess)
    {
        // Espera 2 segundos para mostrar los resultados
        yield return new WaitForSeconds(2f);

        // Comparar la apuesta con el número de dientes apostados
        bool isBetPar = (betAmount % 2 == 0);

        if (dealerGuess == isBetPar)
        {
            dealerTeeth += betAmount;
            playerTeeth -= betAmount;
            dealerText.text = "¡El dealer adivinó correctamente! Te ha ganado " + betAmount + " dientes.";
        }
        else
        {
            dealerTeeth -= betAmount;
            playerTeeth += betAmount;
            dealerText.text = "¡El dealer falló! Has ganado " + betAmount + " dientes.";
        }

        // Actualizar los textos de dientes del jugador y del dealer por separado
        playerTeethText.text = "Dientes del jugador: " + playerTeeth;
        dealerTeethText.text = "Dientes del dealer: " + dealerTeeth;

        // Esperar 2 segundos más para que el jugador pueda leer el mensaje
        yield return new WaitForSeconds(2f);

        // Verificar condiciones de victoria o derrota
        if (playerTeeth >= 20)
        {
            dealerText.text = "¡Ganaste! ¡Tienes 20 dientes!";
            gameOver = true;
            StartCoroutine(EndGame());
        }
        else if (playerTeeth <= 0)
        {
            dealerText.text = "¡Perdiste! Te quedaste sin dientes...";
            gameOver = true;
            StartCoroutine(EndGame());
        }
        else
        {
            // Volver al turno del dealer
            StartCoroutine(DealerTurn());
        }
    }

    // Función que se llama al final del juego (ganar o perder) para cargar la escena principal
    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(2f);  // Esperar antes de cargar la escena
        SceneManager.LoadScene("MainScene");  // Cambiar "MainScene" por el nombre de tu escena principal
    }
}