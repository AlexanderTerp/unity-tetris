using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour, IGameState
{
    private static GameState Instance;

    public event Action GameStartedEvent;
    public event Action GameOverEvent;
    public event Action<bool> GamePausedEvent;
    public event Action<HighScoreInfo> NewHighScoreInfoEvent;
    public event Action<TetrisScene> SceneSwitchEvent;
    public event Action<int> RowsCompletedEvent;

    public float Difficulty { get { return _difficulty; } set { _difficulty = value; } }

    [Range(0, 1f)]
    [SerializeField] private float _difficulty;

    private bool _isGameInProgress = true;
    private bool _isGameOver;
    private bool _isGamePaused = false;
    private HighScoreManager _highScoreManager;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this);

        SaveManager saveManager = SaveManager.CreateAndLoad();
        _highScoreManager = new HighScoreManager(saveManager);
    }

    public void SetGameStarted()
    {
        OnGameStarted();
        EventUtil.SafeInvoke(GameStartedEvent);
    }

    public void SetGameOver()
    {
        OnGameOver();
        EventUtil.SafeInvoke(GameOverEvent);
    }

    public void ToggleGamePaused()
    {
        OnGamePaused();
        EventUtil.SafeInvoke(GamePausedEvent, _isGamePaused);
    }

    public bool IsGameInProgress()
    {
        return _isGameInProgress;
    }

    public bool IsGameOver()
    {
        return _isGameOver;
    }

    public bool IsPaused()
    {
        return _isGamePaused;
    }

    public void SaveScore(HighScoreInfo highScoreInfo)
    {
        _highScoreManager.Save(highScoreInfo);
        EventUtil.SafeInvoke(NewHighScoreInfoEvent, highScoreInfo);
    }

    public List<HighScoreInfo> GetHighScores()
    {
        return _highScoreManager.GetHighScores();
    }

    public void SwitchToScene(TetrisScene scene)
    {
        EventUtil.SafeInvoke(SceneSwitchEvent, scene);

        GameStartedEvent = null;
        GameOverEvent = null;
        GamePausedEvent = null;
        NewHighScoreInfoEvent = null;

        SceneManager.LoadScene(scene.Name());
    }

    public void NotifyRowCompletion(int numRowsCompleted)
    {
        EventUtil.SafeInvoke(RowsCompletedEvent, numRowsCompleted);
    }

    public void DeleteHighScores()
    {
        _highScoreManager.DeleteHighScores();
    }

    private void OnGameStarted()
    {
        _isGameInProgress = true;
        _isGameOver = false;
        _isGamePaused = false;
        Debug.Log("Game started.");
    }

    private void OnGameOver()
    {
        _isGameInProgress = false;
        _isGameOver = true;
        Debug.Log("Game over.");
    }

    private void OnGamePaused()
    {
        _isGamePaused = !_isGamePaused;
        Debug.Log(_isGamePaused ? "Game paused." : "Game unpaused");
    }
}
