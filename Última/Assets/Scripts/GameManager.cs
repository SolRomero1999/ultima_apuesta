using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool postRuletaDialogsSeen = false;
    private bool isPlayerJudge = false;
    private int playerState = 0;
    private int currentJudgeLevel = 0; // 0 = bartender original, 1 = primer jugador, etc.

    private List<string> minigamesOrder = new List<string>
    {
        "Par_Impar",
        "Ruleta_Rusa"
    };

    private List<string> completedMinigames = new List<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            LoadJudgeLevel();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadJudgeLevel()
    {
        currentJudgeLevel = PlayerPrefs.GetInt("CurrentJudgeLevel", 0);
    }

    public int GetPlayerState()
    {
        return playerState;
    }

    public void SetPlayerState(int state)
    {
        playerState = state % 3;
        Debug.Log("Estado del jugador actualizado en GameManager: " + playerState);
    }

    public void ChangePlayerState()
    {
        int newState = (playerState + 1) % 3;
        SetPlayerState(newState);
    }

    public bool CanPlay(string sceneName)
    {
        if (!minigamesOrder.Contains(sceneName)) return true;

        if (sceneName == minigamesOrder[0] && completedMinigames.Count == 0)
            return true;

        int index = minigamesOrder.IndexOf(sceneName);
        if (index > 0 && completedMinigames.Contains(minigamesOrder[index - 1]))
            return true;

        return false;
    }

    public void CompleteMinigame(string sceneName)
    {
        if (!completedMinigames.Contains(sceneName))
        {
            completedMinigames.Add(sceneName);
            Debug.Log("Minijuego completado: " + sceneName);
        }
    }

    public bool IsMinigameCompleted(string sceneName)
    {
        return completedMinigames.Contains(sceneName);
    }

    public bool HasSeenPostRuletaDialogs()
    {
        return postRuletaDialogsSeen;
    }

    public void MarkPostRuletaDialogsSeen()
    {
        postRuletaDialogsSeen = true;
    }

    public void SetPlayerAsJudge()
    {
        isPlayerJudge = true;
    }

    public bool IsPlayerJudge()
    {
        return isPlayerJudge;
    }

    public int GetCurrentJudgeLevel()
    {
        return currentJudgeLevel;
    }

    public void SetNewJudge(int playerState)
    {
        currentJudgeLevel = playerState + 1;
        PlayerPrefs.SetInt("CurrentJudgeLevel", currentJudgeLevel);
        Debug.Log("Nuevo juez establecido. Nivel: " + currentJudgeLevel);
    }
}
