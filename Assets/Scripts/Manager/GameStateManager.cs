using Score;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Calibration, Playing, Paused, GameOver }

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurState { get; private set; }

    [SerializeField] private InputActionReference _pauseAction;

    private GameObject _pausePanel;
    private PlayerLives _playerLives;
    private HorseMovement _horseMovement;
    private EnemySpawner[] _enemySpawners;

    private const string CalibrationSceneName = "hammerTest";
    private bool _calibrationOverlayActive;
    private Camera _overlayCachedCamera;
    private AudioListener _overlayCachedAudio;
    private EventSystem _overlayCachedEventSystem;
    private Canvas _overlayCachedHud;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        FindSceneReferences();
    }

    private void OnEnable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (_pauseAction?.action != null)
        {
            _pauseAction.action.performed += OnPausePerformed;
            _pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_pauseAction?.action != null)
        {
            _pauseAction.action.performed -= OnPausePerformed;
            _pauseAction.action.Disable();
        }
        if (_playerLives != null) _playerLives.OnGameOver -= HandleGameOver;
    }

    private void FindSceneReferences()
    {
        _enemySpawners = FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None);
        _horseMovement = FindFirstObjectByType<HorseMovement>();

        if (_playerLives != null) _playerLives.OnGameOver -= HandleGameOver;
        _playerLives = FindFirstObjectByType<PlayerLives>();
        if (_playerLives != null) _playerLives.OnGameOver += HandleGameOver;

        // Pause Menu starts inactive; FindObjectsOfTypeAll includes inactive,
        // filter by active scene to skip prefab assets.
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        Time.timeScale = 1f;
        FindSceneReferences();
        switch (scene.name)
        {
            case "MainScene":
                ScoreManager.Instance?.ResetFear();
                ScoreManager.Instance?.ResetAwe();
                ApplyState(GameState.Playing);
                break;
            case "MainMenu":
                CurState = GameState.Menu;
                break;
            case "hammerTest":
                CurState = GameState.Calibration;
                break;
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext _) => TogglePause();

    private void HandleGameOver()
    {
        if (CurState == GameState.Playing) SetState(GameState.GameOver);
    }

    public void SetState(GameState next)
    {
        if (CurState == next) return;
        ApplyState(next);
    }

    // Applies the state's effects unconditionally. Used by SetState and by
    // scene-load to re-configure freshly-loaded references.
    private void ApplyState(GameState next)
    {
        CurState = next;
        switch (next)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                SetSpawning(true);
                if (_horseMovement != null) _horseMovement.enabled = true;
                if (_pausePanel != null) _pausePanel.SetActive(false);
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                SetSpawning(false);
                if (_horseMovement != null) _horseMovement.enabled = false;
                if (_pausePanel != null) _pausePanel.SetActive(true);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                SetSpawning(false);
                break;
        }
    }

    private void SetSpawning(bool enabled)
    {
        if (_enemySpawners == null) return;
        foreach (EnemySpawner s in _enemySpawners) if (s != null) s.spawningEnabled = enabled;
    }

    public void TogglePause()
    {
        if (CurState == GameState.Playing) SetState(GameState.Paused);
        else if (CurState == GameState.Paused) SetState(GameState.Playing);
    }

    // Static entry points: prefab UnityEvents can't reference scene-only objects.
    public static void Button_Resume()
    {
        if (Instance != null && Instance.CurState == GameState.Paused)
            Instance.SetState(GameState.Playing);
    }

    public static void Button_OpenCalibration()
    {
        if (Instance != null) Instance.OpenCalibrationOverlay();
    }

    // hammerTest's "Play Game" button: closes the overlay if mid-game,
    // otherwise (reached from main menu) starts a fresh run.
    public static void Button_CalibrationDone()
    {
        if (Instance != null && Instance._calibrationOverlayActive) Instance.CloseCalibrationOverlay();
        else SceneManager.LoadScene("MainScene");
    }

    private void OpenCalibrationOverlay()
    {
        if (_calibrationOverlayActive) return;
        if (CurState == GameState.Playing) SetState(GameState.Paused);
        if (_pausePanel != null) _pausePanel.SetActive(false);

        // Cache and disable MainScene's camera/audio/input/HUD so hammerTest's don't conflict.
        _overlayCachedCamera = Camera.main;
        _overlayCachedAudio = FindFirstObjectByType<AudioListener>();
        _overlayCachedEventSystem = FindFirstObjectByType<EventSystem>();
        foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            if (root.name == "Main HUD Canvas") { _overlayCachedHud = root.GetComponent<Canvas>(); break; }

        if (_overlayCachedCamera != null)      _overlayCachedCamera.enabled = false;
        if (_overlayCachedAudio != null)       _overlayCachedAudio.enabled = false;
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = false;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = false;

        _calibrationOverlayActive = true;
        SceneManager.LoadScene(CalibrationSceneName, LoadSceneMode.Additive);
    }

    private void CloseCalibrationOverlay()
    {
        if (!_calibrationOverlayActive) return;

        Scene overlay = SceneManager.GetSceneByName(CalibrationSceneName);
        if (overlay.IsValid() && overlay.isLoaded) SceneManager.UnloadSceneAsync(overlay);

        if (_overlayCachedCamera != null)      _overlayCachedCamera.enabled = true;
        if (_overlayCachedAudio != null)       _overlayCachedAudio.enabled = true;
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = true;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = true;

        _overlayCachedCamera = null;
        _overlayCachedAudio = null;
        _overlayCachedEventSystem = null;
        _overlayCachedHud = null;

        _calibrationOverlayActive = false;
        if (_pausePanel != null) _pausePanel.SetActive(true);
    }
}
