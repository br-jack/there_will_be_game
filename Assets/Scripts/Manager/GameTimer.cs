using TMPro;
using UnityEngine;

// Counts down from `duration` seconds while the game is in the Playing state,
// updating a TMP_Text with an mm:ss readout. When it reaches zero, transitions
// GameStateManager into GameOver, which triggers the existing GameOverUI.Show
// flow.
public class GameTimer : MonoBehaviour
{
    [SerializeField] private float duration = 300f;   // 5 minutes
    [SerializeField] private TMP_Text timerText;

    private float _timeRemaining;
    private bool _finished;

    private void Awake()
    {
        _timeRemaining = duration;
        UpdateText();
    }

    private void Update()
    {
        if (_finished) return;

        // Freeze the timer during pause and game-over. Other states (including
        // the default Menu state when pressing Play directly on MainScene in
        // the editor) still tick — Time.timeScale=0 during pause freezes
        // Time.deltaTime anyway, the explicit check is belt-and-braces.
        GameStateManager gsm = GameStateManager.Instance;
        if (gsm != null && (gsm.CurState == GameState.Paused || gsm.CurState == GameState.GameOver)) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _finished = true;
            UpdateText();
            gsm.SetState(GameState.GameOver);
            return;
        }

        UpdateText();
    }

    private void UpdateText()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
