using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
    }

    private void OnDestroy()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
        }
    }

    private void StartGame()
    {
        UpdatePlayerState();
        LoadMainScene();
    }

    private void UpdatePlayerState()
    {
        if (GameManager.Instance == null) return;

        int currentState = GameManager.Instance.GetPlayerState();
        GameManager.Instance.SetPlayerState(currentState + 1);
    }

    private void LoadMainScene()
    {
        SceneManager.LoadScene("MainScene");
    }
}