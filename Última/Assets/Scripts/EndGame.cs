using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    public Button startButton;

    private void Start()
    {
        startButton.onClick.AddListener(StartGame);
    }

    private void StartGame()
    {
        if (GameManager.instance != null)
        {
            int newState = GameManager.instance.GetPlayerState() + 1;
            GameManager.instance.SetPlayerState(newState);
        }

        SceneManager.LoadScene("MainScene");
    }
}
