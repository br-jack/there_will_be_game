using Hammer;
using UnityEngine;

public enum GameState
{
    Menu,
    Calibration,
    BeforePlay,
    Playing,
    Paused,
    GameOver

}

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public static HammerBehaviour HammerBehaviour { get; private set; }
    public GameState CurState { get; private set; }
    public float playTimeSoFar { get; private set; }

    // Scene references
    private PlayerLives _playerLives;
    private HorseMovement _horseMovement;
    private EnemySpawner[] _EnemySpawners;
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
            ElapsedRunTime += Time.deltaTime;
        }
    }

    public void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    public void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
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
            // Valid Transitions from PreRun
            (GameState.BeforePlay, GameState.Running) => true,
            _ => false
        };

        if (!isValid)
        {
            Debug.LogWarning($"{CurrentState} -> {next} is an invalid game state transition.");
            return;
        }

        GameState prev = CurState;
        CurState = next;
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
        // If a scene just loaded, force the state to match the scene
        // This has no transition unlike calling SetState()
        switch (scene.name)
        {
            case "MainScene":
                SetState(GameState.PreRun);
                break;
            case "MainMenu":
                OverrideGameState(GameState.Menu);
                break;
            case "hammerTest":
                OverrideGameState(GameState.calibration);
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
        if (_spawners == null) return;

        foreach (EnemySpawner spawner in _spawners)
        {
            if (spawner == null) return;
            spawner.spawningEnabled = true;
        }
    }

    private void DisableSpawning()
    {
        if (_spawners == null) return;

        foreach (EnemySpawner spawner in _spawners)
        {
            if (spawner == null) return;
            spawner.spawningEnabled = false;
        }
    }

    private void EnterBeforePlaying()
    {
        Time.timeScale = 1f;
        ElapsedRunTime = 0f;

        EnableSpawning();
    }

    private void EnterPlaying()
    {
        Time.timeScale = 1f;
        DisableSpawning();

        _pausePanel?.SetActive(false);
    }

    private void EnterPaused()
    {
        Time.timeScale = 0f;
        SetPlayerInputEnabled(false);

        if (_pausePanel != null) _pausePanel.SetActive(false);
    }

    private void EnterGameOver()
    {
        Time.timeScale = 0f;
        DisableSpawning();
    }
}
