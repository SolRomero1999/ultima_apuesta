using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Button startButton;  

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        SceneManager.LoadScene("MainScene");  
    }
}