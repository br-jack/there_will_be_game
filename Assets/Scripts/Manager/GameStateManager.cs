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
    public float playTimeSoFar { get; private set; }

    // Pause is bound to a button (Escape) in the input asset; assign on the prefab.
    [SerializeField] private InputActionReference _pauseAction;

    // Scene references
    [SerializeField] private GameObject _pausePanel;
    private PlayerLives _playerLives;
    private HorseMovement _horseMovement;
    private EnemySpawner[] _enemySpawners;

    // Additive calibration overlay state. When active, hammerTest is loaded
    // on top of MainScene; these fields remember what we disabled so we can
    // restore it exactly when the overlay closes.
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

    // Finds per-scene references. Called on Awake and after every scene load
    // because DontDestroyOnLoad means the new scene's objects need re-discovering.
    private void FindSceneReferences()
    {
        _enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        _horseMovement = FindFirstObjectByType<HorseMovement>();

        // Unsubscribe from any prior PlayerLives before swapping refs.
        // Use `is not null` to bypass Unity's overloaded == that returns true for destroyed objects.
        if (_playerLives is not null) _playerLives.OnGameOver -= HandleGameOver;
        _playerLives = FindFirstObjectByType<PlayerLives>();
        if (_playerLives != null) _playerLives.OnGameOver += HandleGameOver;

        // Pause Menu starts inactive in the prefab, so the standard
        // FindObject* APIs won't return it. Walk the active scene's
        // root objects and look for it by name.
        _pausePanel = null;
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
        {
            Transform match = FindChildRecursive(root.transform, "Pause Menu");
            if (match != null)
            {
                _pausePanel = match.gameObject;
                break;
            }
        }
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform hit = FindChildRecursive(parent.GetChild(i), name);
            if (hit != null) return hit;
        }
        return null;
    }

    private void Start()
    {
        // SceneManager.sceneLoaded does NOT fire for the initial scene, so
        // drive the handler manually once on startup to set the correct state.
        NewSceneJustLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
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
        // Skip wiring if this is a duplicate singleton instance about to be destroyed —
        // touching the shared InputAction here would interfere with the real instance.
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

    private void OnPausePerformed(InputAction.CallbackContext _) => TogglePause();

    private void HandleGameOver()
    {
        if (CurState == GameState.Playing) SetState(GameState.GameOver);
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

    // --- Static entry points for UI buttons ---
    // UnityEvents in prefabs can't reference scene-only objects, so the
    // pause menu buttons call these statics instead of an instance.

    public static void Button_TogglePause()
    {
        if (Instance != null) Instance.TogglePause();
    }

    public static void Button_Resume()
    {
        if (Instance != null && Instance.CurState == GameState.Paused)
            Instance.SetState(GameState.Playing);
    }

    public static void Button_Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainScene");
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

    // In-place calibration — samples the hammer's current orientation and
    // stores its inverse on the persistent HammerManager singleton. Safe to
    // call while paused; no scene change, run continues on resume.
    public static void Button_CalibrateHammer()
    {
        var hammer = FindFirstObjectByType<Hammer.HammerBehaviour>();
        if (hammer != null)
        {
            hammer.CalibrateHammer();
        }
        else
        {
            Debug.LogWarning("Button_CalibrateHammer: no HammerBehaviour found in the active scene.");
        }
    }

    // ----- Additive calibration overlay -----
    // Loads hammerTest on top of MainScene so the run stays alive in memory.
    // MainScene's view/audio/input/HUD are temporarily disabled so the two
    // scenes don't fight over cameras, audio listeners, or the event system.

    public static void Button_OpenCalibration()
    {
        if (Instance != null) Instance.OpenCalibrationOverlay();
    }

    public static void Button_CloseCalibration()
    {
        if (Instance != null) Instance.CloseCalibrationOverlay();
    }

    // Used by hammerTest's "Play Game" button. If the scene is up as an
    // overlay, close it and resume. Otherwise (reached from the main menu as
    // a normal scene) start a fresh run.
    public static void Button_CalibrationDone()
    {
        if (Instance != null && Instance._calibrationOverlayActive)
        {
            Instance.CloseCalibrationOverlay();
        }
        else
        {
            Button_PlayGame();
        }
    }

    private void OpenCalibrationOverlay()
    {
        if (_calibrationOverlayActive) return;
        if (SceneManager.GetSceneByName(CalibrationSceneName).IsValid()) return;

        // Enter Paused so the state machine knows we're suspended. If we're
        // coming from Playing this is a valid transition; if we're already
        // Paused, skip the transition.
        if (CurState == GameState.Playing) SetState(GameState.Paused);

        // Hide the pause panel — hammerTest will own the screen.
        if (_pausePanel != null) _pausePanel.SetActive(false);

        // Cache and disable MainScene's rendering, audio, input, and HUD so
        // hammerTest's equivalents don't conflict.
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

        // Restore MainScene's rendering, audio, input, and HUD.
        if (_overlayCachedCamera != null) _overlayCachedCamera.enabled = true;
        if (_overlayCachedAudio != null) _overlayCachedAudio.enabled = true;
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = true;
        if (_overlayCachedHud != null) _overlayCachedHud.enabled = true;

        _overlayCachedCamera = null;
        _overlayCachedAudio = null;
        _overlayCachedEventSystem = null;
        _overlayCachedHud = null;

        _calibrationOverlayActive = false;

        // Return to the pause menu so the player can resume or tweak settings.
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
        // Additive loads are overlays (e.g. mid-game calibration). They don't
        // own the game state, and MainScene's refs are still valid, so skip
        // rediscovery and the state override.
        if (mode == LoadSceneMode.Additive) return;

        // Re-discover scene refs: since this manager is DontDestroyOnLoad, the
        // refs from the previous scene now point at destroyed objects.
        FindSceneReferences();

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
        CurState = next;
        EnterState(next);
    }

    private void EnableSpawning()
    {
        if (_enemySpawners == null) return;

        foreach (EnemySpawner spawner in _enemySpawners)
        {
            if (spawner == null) continue;
            spawner.spawningEnabled = true;
        }
    }


    // Enable and Disable the spawners (mainly used as helper functions)
    private void DisableSpawning()
    {
        if (_enemySpawners == null) return;

        foreach (EnemySpawner spawner in _enemySpawners)
        {
            if (spawner == null) continue;
            spawner.spawningEnabled = false;
        }
    }

    // Enable/disable the player's input-driven movement.
    private void SetPlayerInputEnabled(bool enabled)
    {
        if (_horseMovement != null) _horseMovement.enabled = enabled;
    }

    // Functions for entering specific GameStates
    private void EnterPlaying()
    {
        Time.timeScale = 1f;
        EnableSpawning();
        SetPlayerInputEnabled(true);

        _pausePanel?.SetActive(false);
    }

    private void EnterBeforePlaying()
    {
        Time.timeScale = 1f;
        playTimeSoFar = 0f;

        ScoreManager.Instance?.ResetFear();
        ScoreManager.Instance?.ResetAwe();

        EnableSpawning();

        // Auto-advance to Playing — there's currently no UI gate between
        // scene load and gameplay, so collapse the transition immediately.
        SetState(GameState.Playing);
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
