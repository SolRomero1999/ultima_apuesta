using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;  // Botón para empezar el juego
    public Button quitButton;   // Botón para salir del juego

    public void Start()
    {
        // Asignar funciones a los botones
        startButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Método para comenzar el juego
    public void StartGame()
    {
        SceneManager.LoadScene("MainScene");  // Cargar la escena principal
    }

    // Método para salir del juego
    public void QuitGame()
    {
        Application.Quit();  // Cerrar la aplicación
    }
}
