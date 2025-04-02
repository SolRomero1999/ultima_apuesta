using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Scripting;

public class MainMenuController : MonoBehaviour
{
    public Button startButton;  

    private void Start()
    {
        PlayerPrefs.DeleteKey("PreviousScene");
        PlayerPrefs.DeleteKey("CurrentJudgeLevel");
        PlayerPrefs.DeleteKey("PlayerState");
        InvokeRepeating(nameof(CleanTempAlloc), 60f, 60f);
        startButton.onClick.AddListener(StartGame);

        if (GameManager.Instance != null)
        {
            Destroy(GameManager.Instance.gameObject);
        }
    }
    
    [Preserve]
    void CleanTempAlloc()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        UnityEngine.Resources.UnloadUnusedAssets();
    }

    private void StartGame()
    {
        PlayerPrefs.SetString("PreviousScene", "MainMenu");
        PlayerPrefs.Save(); 
        SceneManager.LoadScene("MainScene");  
    }
}