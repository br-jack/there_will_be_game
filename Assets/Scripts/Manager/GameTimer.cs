using TMPro;
using UnityEngine;

// Edit duration to edit the time that counts down.
// There's a text reference to assign so that the correct mm::ss is displayed while the game plays
// When timer reaches 0, it transitions the GameStateManager into GameOver, which shows the GameOver panel.
public class GameTimer : MonoBehaviour
{
    [SerializeField] private float duration = 300f;
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
