using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private List<string> minigamesOrder = new List<string>
    {
        "Par_Impar",
        "Ruleta_Rusa"
    };

    private int currentMinigameIndex = 0;

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
        if (currentMinigameIndex < minigamesOrder.Count)
        {
            return minigamesOrder[currentMinigameIndex] == sceneName;
        }
        return false;
    }

    public void AdvanceProgress(string sceneName)
    {
        if (currentMinigameIndex < minigamesOrder.Count && minigamesOrder[currentMinigameIndex] == sceneName)
        {
            currentMinigameIndex++;
        }
    }
}
