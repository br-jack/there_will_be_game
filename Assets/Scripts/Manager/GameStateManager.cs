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
    private GameOverUI _gameOverUI;
    private PlayerLives _playerLives;
    private horseMovementGaits _horseMovement;
    private EnemySpawner[] _enemySpawners;

    private const string CalibrationSceneName = "hammerTest";
    private bool _calibrationOverlayActive;
    // Cache ALL cameras and listeners active in the base scene so we can
    // fully silence them (GameObject-level) while the overlay is up.
    private System.Collections.Generic.List<GameObject> _overlayCachedCameraObjects = new System.Collections.Generic.List<GameObject>();
    private System.Collections.Generic.List<GameObject> _overlayCachedAudioObjects = new System.Collections.Generic.List<GameObject>();
    // Hide MainScene geometry so hammerTest's camera renders identically
    // whether loaded standalone or additively. Terrain needs its own list
    // because Unity's Terrain component isn't a Renderer subclass.
    private System.Collections.Generic.List<Renderer> _overlayCachedRenderers = new System.Collections.Generic.List<Renderer>();
    private System.Collections.Generic.List<Terrain> _overlayCachedTerrains = new System.Collections.Generic.List<Terrain>();
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
        _horseMovement = FindFirstObjectByType<horseMovementGaits>();

        if (_playerLives != null) _playerLives.OnGameOver -= HandleGameOver;
        _playerLives = FindFirstObjectByType<PlayerLives>();
        if (_playerLives != null) _playerLives.OnGameOver += HandleGameOver;

        // Pause Menu and GameOverPanel start inactive; FindObjectsOfTypeAll
        // includes inactive, filter by active scene to skip prefab assets.
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

    private void OnPausePerformed(InputAction.CallbackContext _)
    {
        Debug.Log($"[GSM] OnPausePerformed fired. CurState={CurState}, frame={Time.frameCount}");
        TogglePause();
    }

    private void HandleGameOver()
    {
        if (CurState == GameState.Playing) SetState(GameState.GameOver);
    }

    public void SetState(GameState next)
    {
        Debug.Log($"[GSM] SetState called: {CurState} -> {next}, frame={Time.frameCount}, caller stack:\n{System.Environment.StackTrace}");
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
            case GameState.Calibration:
                // Keep timeScale at 1 so TargetHammer's FixedUpdate simulation
                // runs — the hammer must animate during calibration identically
                // to how it does when hammerTest is loaded standalone.
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
        if (CurState == GameState.Playing) SetState(GameState.Paused);
        else if (CurState == GameState.Paused) SetState(GameState.Playing);
    }

    // Static entry points: prefab UnityEvents can't reference scene-only objects.
    public static void Button_Pause()
    {
        if (Instance != null && Instance.CurState == GameState.Playing)
            Instance.SetState(GameState.Paused);
    }

    public static void Button_Resume()
    {
        if (Instance != null && Instance.CurState == GameState.Paused)
            Instance.SetState(GameState.Playing);
    }

    public static void Button_OpenCalibration()
    {
        Debug.Log($"[GSM] Button_OpenCalibration clicked. Instance={(Instance == null ? "NULL" : "ok")}");
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
        Debug.Log($"[GSM] OpenCalibrationOverlay: overlayActive={_calibrationOverlayActive}, curState={CurState}");
        if (_calibrationOverlayActive) { Debug.Log("[GSM] Already active — returning early."); return; }
        if (CurState == GameState.Playing) SetState(GameState.Paused);
        if (_pausePanel != null) _pausePanel.SetActive(false);

        // Fully silence the base scene: disable every camera and audio listener
        // at the GameObject level so follow/controller scripts on them stop
        // running too. Input (EventSystem) and HUD stay component-disabled.
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

        // MainScene renderers and terrain stay enabled so the player sees the
        // gameplay world as a backdrop behind the calibration hammer. The
        // base-scene camera is disabled above; hammerTest's camera renders the
        // world it can see through its frustum plus the hammer on top.
        _overlayCachedRenderers.Clear();
        _overlayCachedTerrains.Clear();

        _overlayCachedEventSystem = FindFirstObjectByType<EventSystem>();
        foreach (GameObject root in activeScene.GetRootGameObjects())
            if (root.name == "Main HUD Canvas") { _overlayCachedHud = root.GetComponent<Canvas>(); break; }

        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = false;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = false;

        _calibrationOverlayActive = true;
        // Force timeScale=1 while the overlay is up so TargetHammer's FixedUpdate
        // runs and the hammer animates identically to when hammerTest is loaded
        // standalone from the main menu. The base scene is logically Paused;
        // CloseCalibrationOverlay restores its timescale on the way out.
        // (OnSceneLoaded's ApplyState(Calibration) branch handles the Single-load
        // path, but it early-returns on Additive — hence this explicit set.)
        Time.timeScale = 1f;
        Debug.Log($"[GSM] About to LoadScene additive: {CalibrationSceneName}");
        SceneManager.LoadScene(CalibrationSceneName, LoadSceneMode.Additive);
        Debug.Log("[GSM] LoadScene call returned.");
        StartCoroutine(LogOverlayCameraState());
    }

    private System.Collections.IEnumerator LogOverlayCameraState()
    {
        yield return null; // let additive scene finish loading
        Scene overlay = SceneManager.GetSceneByName(CalibrationSceneName);
        foreach (GameObject root in overlay.GetRootGameObjects())
            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
            {
                Debug.Log($"[GSM] hammerTest cam '{cam.name}' pos={cam.transform.position} rot={cam.transform.eulerAngles} fov={cam.fieldOfView} rect={cam.rect} pixelRect={cam.pixelRect} targetDisplay={cam.targetDisplay} aspect={cam.aspect}");
            }
        Debug.Log($"[GSM] Screen resolution: {Screen.width}x{Screen.height}, fullscreen={Screen.fullScreen}");
    }

    private void CloseCalibrationOverlay()
    {
        if (!_calibrationOverlayActive) return;

        Scene overlay = SceneManager.GetSceneByName(CalibrationSceneName);
        if (overlay.IsValid() && overlay.isLoaded) SceneManager.UnloadSceneAsync(overlay);

        foreach (GameObject go in _overlayCachedCameraObjects) if (go != null) go.SetActive(true);
        foreach (GameObject go in _overlayCachedAudioObjects)  if (go != null) go.SetActive(true);
        foreach (Renderer r in _overlayCachedRenderers)        if (r != null) r.enabled = true;
        foreach (Terrain t in _overlayCachedTerrains)          if (t != null) t.enabled = true;
        if (_overlayCachedEventSystem != null) _overlayCachedEventSystem.enabled = true;
        if (_overlayCachedHud != null)         _overlayCachedHud.enabled = true;

        _overlayCachedCameraObjects.Clear();
        _overlayCachedAudioObjects.Clear();
        _overlayCachedRenderers.Clear();
        _overlayCachedTerrains.Clear();
        _overlayCachedEventSystem = null;
        _overlayCachedHud = null;

        _calibrationOverlayActive = false;
        // Restore the base scene's timescale for whatever state it's still in.
        // OpenCalibrationOverlay forced timeScale=1 to animate the hammer; the
        // underlying state (Paused during gameplay-entry, unchanged otherwise)
        // dictates what it should be on close.
        Time.timeScale = (CurState == GameState.Paused || CurState == GameState.GameOver) ? 0f : 1f;
        if (_pausePanel != null) _pausePanel.SetActive(true);
    }
}
