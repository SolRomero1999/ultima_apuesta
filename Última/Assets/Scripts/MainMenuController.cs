using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;  

    private void Start()
    {
        PlayerPrefs.DeleteKey("PreviousScene");
        PlayerPrefs.DeleteKey("CurrentJudgeLevel");
        PlayerPrefs.DeleteKey("PlayerState");
        startButton.onClick.AddListener(StartGame);
        
        if (GameManager.instance != null)
        {
            // Esto destruirá el GameManager existente para empezar fresco
            Destroy(GameManager.instance.gameObject);
        }
    }

    private void StartGame()
    {
        PlayerPrefs.SetString("PreviousScene", "MainMenu");
        PlayerPrefs.Save(); 
        SceneManager.LoadScene("MainScene");  
    }
}