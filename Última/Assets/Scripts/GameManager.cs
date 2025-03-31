using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool postRuletaDialogsSeen = false;
    private bool isPlayerJudge = false; // Estado para determinar si el jugador es juez.
    private int playerState = 0; // Estado actual del jugador (0, 1, 2)

    // Orden de los minijuegos
    private List<string> minigamesOrder = new List<string>
    {
        "Par_Impar",
        "Ruleta_Rusa"
    };

    // Lista de minijuegos completados
    private List<string> completedMinigames = new List<string>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Método para obtener el estado del jugador
    public int GetPlayerState()
    {
        return playerState;
    }

    // Método para establecer el estado del jugador
    public void SetPlayerState(int state)
    {
        playerState = state % 3; // Asegura que el estado se mantenga entre 0 y 2
        Debug.Log("Estado del jugador actualizado en GameManager: " + playerState);
    }

    // Método para cambiar el estado del jugador desde otros scripts
    public void ChangePlayerState()
    {
        int newState = (playerState + 1) % 3; // Cicla entre 0, 1 y 2
        SetPlayerState(newState); // Actualiza el estado en GameManager
    }

    // Verifica si un minijuego puede ser jugado
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

    // Marca un minijuego como completado
    public void CompleteMinigame(string sceneName)
    {
        if (!completedMinigames.Contains(sceneName))
        {
            completedMinigames.Add(sceneName);
            Debug.Log("Minijuego completado: " + sceneName);
        }
    }

    // Verifica si un minijuego ha sido completado
    public bool IsMinigameCompleted(string sceneName)
    {
        return completedMinigames.Contains(sceneName);
    }

    // Verifica si los diálogos posteriores a la ruleta han sido vistos
    public bool HasSeenPostRuletaDialogs()
    {
        return postRuletaDialogsSeen;
    }

    // Marca los diálogos posteriores a la ruleta como vistos
    public void MarkPostRuletaDialogsSeen()
    {
        postRuletaDialogsSeen = true;
    }

    // Establece al jugador como juez
    public void SetPlayerAsJudge()
    {
        isPlayerJudge = true;
    }

    // Verifica si el jugador es juez
    public bool IsPlayerJudge()
    {
        return isPlayerJudge;
    }
}
