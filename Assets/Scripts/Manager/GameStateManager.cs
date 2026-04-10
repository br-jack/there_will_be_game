using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Calibration,
    BeforePlay,
    Playing,
    Paused,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    public GameState CurState { get; private set; }
    public float playTimeSoFar { get; private set; }

    // Scene references
    private PlayerLives _playerLives;
    private HorseMovement _horseMovement;
    private EnemySpawner[] _enemySpawners;
    private GameObject _pausePanel;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (CurState == GameState.Playing)
        {
            playTimeSoFar += Time.deltaTime;
        }
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += NewSceneJustLoaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= NewSceneJustLoaded;
    }

    public void SetState(GameState next)
    {
        bool isValid = (CurState, next) switch
        {
            // Valid Transitions from Running
            (GameState.Playing, GameState.Paused) => true,
            (GameState.Playing, GameState.GameOver) => true,
            // Valid Transitions from Paused
            (GameState.Paused, GameState.Playing) => true,
            (GameState.Paused, GameState.Menu) => true,
            // Valid Transitions from GameOver
            (GameState.GameOver, GameState.BeforePlay) => true,
            (GameState.GameOver, GameState.Menu) => true,
            // Valid Transitions from Menu
            (GameState.Menu, GameState.BeforePlay) => true,
            (GameState.Menu, GameState.Calibration) => true,
            // Valid Transitions from Calibration
            (GameState.Calibration, GameState.Menu) => true,
            // Valid Transitions from BeforePlay
            (GameState.BeforePlay, GameState.Playing) => true,
            _ => false
        };

        if (!isValid)
        {
            Debug.LogWarning($"{CurState} -> {next} is an invalid game state transition.");
            return;
        }

        GameState prev = CurState;
        CurState = next;

        ExitState(prev);
        EnterState(next);
    }


    // General functions for entering and exiting states.
    private void ExitState(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                ExitPaused();
                break;
        }
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.BeforePlay:
                EnterBeforePlaying();
                break;
            case GameState.Playing:
                EnterPlaying();
                break;
            case GameState.Paused:
                EnterPaused();
                break;
            case GameState.GameOver:
                EnterGameOver();
                break;
        }
    }

    // Call this function to Pause and Resume the Game
    public void TogglePause()
    {
        if (CurState == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
        else if (CurState == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    private void NewSceneJustLoaded(Scene scene, LoadSceneMode mode)
    {
        // If a scene just loaded, force the state to match the scene.
        // This has no transition unlike calling SetState().
        switch (scene.name)
        {
            case "MainScene":
                OverrideGameState(GameState.BeforePlay);
                break;
            case "MainMenu":
                OverrideGameState(GameState.Menu);
                break;
            case "hammerTest":
                OverrideGameState(GameState.Calibration);
                break;
        }
    }

    private void OverrideGameState(GameState next)
    {
        GameState prev = CurState;
        CurState = next;
    }

    private void EnableSpawning()
    {
        if (_enemySpawners == null) return;

        foreach (EnemySpawner spawner in _enemySpawners)
        {
            if (spawner == null) return;
            spawner.spawningEnabled = true;
        }
    }


    // Enable and Disable the spawners (mainly used as helper functions)
    private void DisableSpawning()
    {
        if (_enemySpawners == null) return;

        foreach (EnemySpawner spawner in _enemySpawners)
        {
            if (spawner == null) return;
            spawner.spawningEnabled = false;
        }
    }

    // Functions for entering specific GameStates
    private void EnterPlaying()
    {
        Time.timeScale = 1f;
        EnableSpawning();

        _pausePanel?.SetActive(false);
    }

    private void EnterBeforePlaying()
    {
        Time.timeScale = 1f;
        ElapsedRunTime = 0f;

        EnableSpawning();
    }

    private void EnterPaused()
    {
        Time.timeScale = 0f;
        DisableSpawning();
        SetPlayerInputEnabled(false);

        if (_pausePanel != null) _pausePanel.SetActive(true);
    }

    private void ExitPaused()
    {
        Time.timeScale = 1f;
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }
    private void EnterGameOver()
    {
        Time.timeScale = 0f;
        DisableSpawning();
    }
}
