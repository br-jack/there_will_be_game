using Score;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
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

    [SerializeField] private InputActionReference _pauseAction;

    [SerializeField] private GameObject _pausePanel;
    private PlayerLives _playerLives;
    private HorseMovement _horseMovement;
    private EnemySpawner[] _enemySpawners;

    // Additive calibration overlay: hammerTest is loaded on top of MainScene
    // and these fields remember what we disabled so we can restore it.
    private const string CalibrationSceneName = "hammerTest";
    private bool _calibrationOverlayActive;
    private Camera _overlayCachedCamera;
    private AudioListener _overlayCachedAudio;
    private EventSystem _overlayCachedEventSystem;
    private Canvas _overlayCachedHud;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        FindSceneReferences();
    }

    // Re-discovers scene refs after every scene load, since DontDestroyOnLoad
    // means refs from the previous scene now point at destroyed objects.
    private void FindSceneReferences()
    {
        _enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        _horseMovement = FindFirstObjectByType<HorseMovement>();

        // `is not null` bypasses Unity's overloaded == that returns true for destroyed objects.
        if (_playerLives is not null) _playerLives.OnGameOver -= HandleGameOver;
        _playerLives = FindFirstObjectByType<PlayerLives>();
        if (_playerLives != null) _playerLives.OnGameOver += HandleGameOver;

        // Pause Menu starts inactive so FindObject* won't return it.
        // Resources.FindObjectsOfTypeAll includes inactive objects but also
        // prefab assets, so filter by the active scene to exclude those.
        _pausePanel = null;
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == "Pause Menu" && t.gameObject.scene == activeScene)
            {
                _pausePanel = t.gameObject;
                break;
            }
        }
    }

    public void OnEnable()
    {
        // Skip wiring on a duplicate singleton about to be destroyed.
        if (Instance != null && Instance != this) return;

        SceneManager.sceneLoaded += NewSceneJustLoaded;

        if (_pauseAction != null && _pauseAction.action != null)
        {
            _pauseAction.action.performed += OnPausePerformed;
            _pauseAction.action.Enable();
        }
    }

    public void OnDisable()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= NewSceneJustLoaded;

        if (_pauseAction != null && _pauseAction.action != null)
        {
            _pauseAction.action.performed -= OnPausePerformed;
            _pauseAction.action.Disable();
        }

        if (_playerLives is not null) _playerLives.OnGameOver -= HandleGameOver;
    }

    private void OnPausePerformed(InputAction.CallbackContext _)
    {
        TogglePause();
    }

    private void HandleGameOver()
    {
        if (CurState == GameState.Playing) SetState(GameState.GameOver);
    }

    public void SetState(GameState next)
    {
        bool isValid = (CurState, next) switch
        {
            (GameState.Playing, GameState.Paused) => true,
            (GameState.Playing, GameState.GameOver) => true,
            (GameState.Paused, GameState.Playing) => true,
            (GameState.Paused, GameState.Menu) => true,
            (GameState.GameOver, GameState.BeforePlay) => true,
            (GameState.GameOver, GameState.Menu) => true,
            (GameState.Menu, GameState.BeforePlay) => true,
            (GameState.Menu, GameState.Calibration) => true,
            (GameState.Calibration, GameState.Menu) => true,
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

    // Static entry points: UnityEvents in prefabs can't reference scene-only objects.
    public static void Button_Resume()
    {
        if (Instance != null && Instance.CurState == GameState.Paused)
            Instance.SetState(GameState.Playing);
    }

    public static void Button_PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScene");
    }

    public static void Button_LoadIntro()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("IntroScene");
    }

    public static void Button_MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public static void Button_Calibration()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("hammerTest");
    }

    public static void Button_OpenCalibration()
    {
        if (Instance != null) Instance.OpenCalibrationOverlay();
    }

    // hammerTest's "Play Game" button: closes the overlay if mid-game,
    // otherwise (reached from main menu) starts a fresh run.
    public static void Button_CalibrationDone()
    {
        if (Instance != null && Instance._calibrationOverlayActive)
            Instance.CloseCalibrationOverlay();
        else
            Button_PlayGame();
    }

    private void OpenCalibrationOverlay()
    {
        if (_calibrationOverlayActive) return;
        if (SceneManager.GetSceneByName(CalibrationSceneName).IsValid()) return;

        if (CurState == GameState.Playing) SetState(GameState.Paused);
        if (_pausePanel != null) _pausePanel.SetActive(false);

        // Cache and disable MainScene's camera/audio/input/HUD so hammerTest's don't conflict.
        _overlayCachedCamera = Camera.main;
        if (_overlayCachedCamera != null) _overlayCachedCamera.enabled = false;

        _overlayCachedAudio = FindFirstObjectByType<AudioListener>();
        if (_overlayCachedAudio != null) _overlayCachedAudio.enabled = false;

        _overlayCachedEventSystem = FindFirstObjectByType<EventSystem>();
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = false;

        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name == "Main HUD Canvas")
            {
                _overlayCachedHud = root.GetComponent<Canvas>();
                break;
            }
        }
        if (_overlayCachedHud != null) _overlayCachedHud.enabled = false;

        _calibrationOverlayActive = true;
        SceneManager.LoadScene(CalibrationSceneName, LoadSceneMode.Additive);
    }

    private void CloseCalibrationOverlay()
    {
        if (!_calibrationOverlayActive) return;

        Scene overlay = SceneManager.GetSceneByName(CalibrationSceneName);
        if (overlay.IsValid() && overlay.isLoaded)
        {
            SceneManager.UnloadSceneAsync(overlay);
        }

        if (_overlayCachedCamera != null) _overlayCachedCamera.enabled = true;
        if (_overlayCachedAudio != null) _overlayCachedAudio.enabled = true;
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = true;
        if (_overlayCachedHud != null) _overlayCachedHud.enabled = true;

        _overlayCachedCamera = null;
        _overlayCachedAudio = null;
        _overlayCachedEventSystem = null;
        _overlayCachedHud = null;

        _calibrationOverlayActive = false;

        if (_pausePanel != null) _pausePanel.SetActive(true);
    }

    public static void Button_Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }

    private void NewSceneJustLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive loads are overlays (e.g. mid-game calibration); skip rediscovery.
        if (mode == LoadSceneMode.Additive) return;

        FindSceneReferences();

        // Force state to match the scene without going through SetState's transition check.
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
        CurState = next;
        EnterState(next);
    }

    private void SetSpawningEnabled(bool enabled)
    {
        if (_enemySpawners == null) return;

        foreach (EnemySpawner spawner in _enemySpawners)
        {
            if (spawner != null) spawner.spawningEnabled = enabled;
        }
    }

    private void SetPlayerInputEnabled(bool enabled)
    {
        if (_horseMovement != null) _horseMovement.enabled = enabled;
    }

    private void EnterPlaying()
    {
        Time.timeScale = 1f;
        SetSpawningEnabled(true);
        SetPlayerInputEnabled(true);

        // Ensure the panel is hidden when entering Playing from any state,
        // including BeforePlay (where ExitPaused doesn't run).
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }

    private void EnterBeforePlaying()
    {
        Time.timeScale = 1f;

        ScoreManager.Instance?.ResetFear();
        ScoreManager.Instance?.ResetAwe();

        SetSpawningEnabled(true);

        // No UI gate between scene load and gameplay, so collapse the transition.
        SetState(GameState.Playing);
    }

    private void EnterPaused()
    {
        Time.timeScale = 0f;
        SetSpawningEnabled(false);
        SetPlayerInputEnabled(false);

        if (_pausePanel != null) _pausePanel.SetActive(true);
    }

    private void ExitPaused()
    {
        if (_pausePanel != null) _pausePanel.SetActive(false);
    }

    private void EnterGameOver()
    {
        Time.timeScale = 0f;
        SetSpawningEnabled(false);
    }
}
