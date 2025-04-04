using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton Pattern
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region Game State Variables
    private bool _postRuletaDialogsSeen = false;
    private bool _isPlayerJudge = false;
    private int _playerState = 0;
    private int _currentJudgeLevel = 0; 
    #endregion

    #region Minigames Management
    private readonly List<string> _minigamesOrder = new List<string>
    {
        "Traga_Monedas",
        "Par_Impar",
        "Ruleta_Rusa"
    };

    private readonly List<string> _completedMinigames = new List<string>();
    #endregion

    #region Initialization
    private void InitializeGameData()
    {
        LoadJudgeLevel();
    }

    private void LoadJudgeLevel()
    {
        _currentJudgeLevel = PlayerPrefs.GetInt("CurrentJudgeLevel", 0);
    }
    #endregion

    #region Player State Management
    public int GetPlayerState() => _playerState;

    public void SetPlayerState(int state)
    {
        _playerState = state % 3;
        Debug.Log($"Estado del jugador actualizado: {_playerState}");
    }

    public void ChangePlayerState()
    {
        SetPlayerState((_playerState + 1) % 3);
    }
    #endregion

    #region Minigame Progress
    public bool CanPlay(string sceneName)
    {
        if (!_minigamesOrder.Contains(sceneName)) return true;

        if (sceneName == _minigamesOrder[0] && _completedMinigames.Count == 0)
            return true;

        int index = _minigamesOrder.IndexOf(sceneName);
        return index > 0 && _completedMinigames.Contains(_minigamesOrder[index - 1]);
    }

    public void CompleteMinigame(string sceneName)
    {
        if (!_completedMinigames.Contains(sceneName))
        {
            _completedMinigames.Add(sceneName);
            Debug.Log($"Minijuego completado: {sceneName}");
        }
    }

    public bool IsMinigameCompleted(string sceneName) => _completedMinigames.Contains(sceneName);
    #endregion

    #region Dialog System
    public bool HasSeenPostRuletaDialogs() => _postRuletaDialogsSeen;

    public void MarkPostRuletaDialogsSeen() => _postRuletaDialogsSeen = true;
    #endregion

    #region Judge System
    public bool IsPlayerJudge() => _isPlayerJudge;

    public void SetPlayerAsJudge() => _isPlayerJudge = true;

    public int GetCurrentJudgeLevel() => _currentJudgeLevel;

    public void SetNewJudge(int playerState)
    {
        _currentJudgeLevel = playerState + 1;
        PlayerPrefs.SetInt("CurrentJudgeLevel", _currentJudgeLevel);
        Debug.Log($"Nuevo juez establecido. Nivel: {_currentJudgeLevel}");
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
    #endregion
}