using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;  

    private void Start()
    {
        PlayerPrefs.DeleteKey("PreviousScene");
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        PlayerPrefs.SetString("PreviousScene", "MainMenu");
        PlayerPrefs.Save(); 
        SceneManager.LoadScene("MainScene");  
    }
}