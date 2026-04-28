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
    public musicManager musicManager;
    private GameObject _pausePanel;
    private GameOverUI _gameOverUI;
    private PlayerLives _playerLives;
    private horseMovementGaits _horseMovement;
    private EnemySpawner[] _enemySpawners;
    [SerializeField] private PauseMenuSelection _pauseMenuSelection;

    private const string CalibrationSceneName = "hammerTest";
    private bool _calibrationOverlayActive;
    private System.Collections.Generic.List<GameObject> _overlayCachedCameraObjects = new System.Collections.Generic.List<GameObject>();
    private System.Collections.Generic.List<GameObject> _overlayCachedAudioObjects = new System.Collections.Generic.List<GameObject>();
    private EventSystem _overlayCachedEventSystem;
    private Canvas _overlayCachedHud;

    public static float ignoreGameplayInputUntil { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        FindSceneReferences();
        InitStateForScene(SceneManager.GetActiveScene().name);
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
        _horseMovement = FindFirstObjectByType<horseMovementGaits>();

        if (_playerLives != null) _playerLives.OnGameOver -= HandleGameOver;
        _playerLives = FindFirstObjectByType<PlayerLives>();
        if (_playerLives != null) _playerLives.OnGameOver += HandleGameOver;

        _pauseMenuSelection = FindFirstObjectByType<PauseMenuSelection>();

        _pausePanel = null;
        _gameOverUI = null;
        Scene activeScene = SceneManager.GetActiveScene();
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.gameObject.scene != activeScene) continue;

            if (t.name == "Pause Menu") _pausePanel = t.gameObject;
            else if (t.name == "GameOverPanel") _gameOverUI = t.GetComponent<GameOverUI>();

            if (_pausePanel != null && _gameOverUI != null) break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive) return;
        Time.timeScale = 1f;
        FindSceneReferences();
        InitStateForScene(scene.name);
    }

    private void InitStateForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainScene":
                ScoreManager.Instance?.ResetFear();
                ScoreManager.Instance?.ResetAwe();
                ApplyState(GameState.Playing);
                
                break;
            case "TutorialScene":
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

    private void OnPausePerformed(InputAction.CallbackContext _)
    {

        Debug.Log("Pause action fired");
        TogglePause();
    }

    private void HandleGameOver()
    {
        if (CurState == GameState.Playing) SetState(GameState.GameOver);
    }

    public void SetState(GameState next)
    {
        if (CurState == next) return;
        ApplyState(next);
    }

    // Applies the state's effects unconditionally. Used by SetState and by scene-load to re-configure freshly-loaded references.
    private void ApplyState(GameState next)
    {
        musicManager = GameObject.Find("MusicManager").GetComponent<musicManager>();
        CurState = next;
        switch (next)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                ignoreGameplayInputUntil = Time.unscaledTime + 0.2f;
                SetSpawning(true);
                musicManager.PlayMusic();
                if (_horseMovement != null) _horseMovement.enabled = true;
                if (_pauseMenuSelection != null) _pauseMenuSelection.ClearSelection();
                if (_pausePanel != null) _pausePanel.SetActive(false);
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                musicManager.PauseMusic();
                SetSpawning(false);
                if (_horseMovement != null) _horseMovement.enabled = false;
                if (_pausePanel != null) _pausePanel.SetActive(true);
                if (_pauseMenuSelection != null) _pauseMenuSelection.SelectFirstButton();
                break;
            case GameState.Calibration:
                // Keep timeScale at 1 so TargetHammer's FixedUpdate simulation
                Time.timeScale = 1f;
                SetSpawning(false);
                if (_horseMovement != null) _horseMovement.enabled = false;
                if (_pausePanel != null) _pausePanel.SetActive(false);
                break;
            case GameState.GameOver:
                Time.timeScale = 0f;
                SetSpawning(false);
                if (_gameOverUI != null)
                {
                    ReportCard tier = ScoreManager.Instance.GetReportCard();
                    _gameOverUI.Show(tier);
                }
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
        Debug.Log($"TogglePause called, current state: {CurState}");
       
        if (CurState == GameState.Playing)
        {
            SetState(GameState.Paused);
        } 
        else if (CurState == GameState.Paused)
        {
            SetState(GameState.Playing);
        } 
            
    }

    // Static entry points: prefab UnityEvents can't reference scene-only objects.
    public static void Button_Pause()
    {
        if (Instance != null && Instance.CurState == GameState.Playing)
        {
            Instance.SetState(GameState.Paused);
        }
            
    }

    public static void Button_Resume()
    {
        if (Instance != null && Instance.CurState == GameState.Paused)
            Instance.SetState(GameState.Playing);
    }

    public static void Button_OpenCalibration()
    {
        if (Instance != null) Instance.OpenCalibrationOverlay();
    }

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

        // Fully silence the base scene.
        Scene activeScene = SceneManager.GetActiveScene();
        _overlayCachedCameraObjects.Clear();
        _overlayCachedAudioObjects.Clear();
        foreach (Camera cam in FindObjectsByType<Camera>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (cam.gameObject.scene == activeScene)
            {
                _overlayCachedCameraObjects.Add(cam.gameObject);
                cam.gameObject.SetActive(false);
            }
        }
        foreach (AudioListener al in FindObjectsByType<AudioListener>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            if (al.gameObject.scene == activeScene)
            {
                _overlayCachedAudioObjects.Add(al.gameObject);
                al.gameObject.SetActive(false);
            }
        }

        _overlayCachedEventSystem = FindFirstObjectByType<EventSystem>();
        foreach (GameObject root in activeScene.GetRootGameObjects())
            if (root.name == "Main HUD Canvas") { _overlayCachedHud = root.GetComponent<Canvas>(); break; }

        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = false;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = false;

        _calibrationOverlayActive = true;

        Time.timeScale = 1f;
        SceneManager.LoadScene(CalibrationSceneName, LoadSceneMode.Additive);
    }

    private void CloseCalibrationOverlay()
    {
        if (!_calibrationOverlayActive) return;

        Scene overlay = SceneManager.GetSceneByName(CalibrationSceneName);
        if (overlay.IsValid() && overlay.isLoaded) SceneManager.UnloadSceneAsync(overlay);

        foreach (GameObject go in _overlayCachedCameraObjects) if (go != null) go.SetActive(true);
        foreach (GameObject go in _overlayCachedAudioObjects)  if (go != null) go.SetActive(true);
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = true;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = true;

        _overlayCachedCameraObjects.Clear();
        _overlayCachedAudioObjects.Clear();
        _overlayCachedEventSystem = null;
        _overlayCachedHud = null;

        _calibrationOverlayActive = false;
        Time.timeScale = (CurState == GameState.Paused || CurState == GameState.GameOver) ? 0f : 1f;
        if (_pausePanel != null) _pausePanel.SetActive(true);
    }
}
