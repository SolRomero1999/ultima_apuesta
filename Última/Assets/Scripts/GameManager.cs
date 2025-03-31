using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private bool postRuletaDialogsSeen = false;

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

    public bool CanPlay(string sceneName)
    {
        // Si no es un minijuego controlado, permitir jugar
        if (!minigamesOrder.Contains(sceneName)) return true;

        // Si es el primer minijuego y no se ha completado ningún minijuego
        if (sceneName == minigamesOrder[0] && completedMinigames.Count == 0)
            return true;

        // Si no es el primer minijuego, verificar si el anterior está completado
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
}