using UnityEngine;

/*
This is purely for TESTING the discrete health bar and the continuos bar.
In the inspector, check the bar/bars that you want to test, then you can watch:
- The discrete hearts count down from 5 to 0
- The continuous bar cycle from 1 to 0 and back to 1

When changing the design of either, this will let you see what it looks like at every bar percentage.
*/
public class BarsTester : MonoBehaviour
{
    [Header("References")]
    public ContinuousBar continuousHealthBar;
    public DiscreteHealthBar discreteHealthBar;

    [Header("Toggle to True to Cycle Through The Bars' States (TEST PURPOSES)")]
    public bool runContinuousTest = true;
    public bool runDiscreteTest = true;

    [Header("Continuous Settings")]
    [Tooltip("Seconds for a full ramp from full to empty and back.")]
    private float continuousCycleSeconds = 4f;

    [Header("Discrete Settings")]
    private int discreteMaxHealth = 5;
    [Tooltip("Seconds between each heart decrement.")]
    private float discreteStepSeconds = 0.8f;

    private float _discreteTimer;
    private int _discreteCurrent;

    private void Start()
    {
        _discreteCurrent = discreteMaxHealth;
    }

    private void Update()
    {
        if (runContinuousTest && continuousHealthBar != null)
        {
            // Ping-pong between full and empty over the cycle time.
            float t = Mathf.PingPong(Time.time, continuousCycleSeconds) / continuousCycleSeconds;
            float fraction = 1f - t;
            continuousHealthBar.DisplayFractionalBar(fraction);
        }

        if (runDiscreteTest && discreteHealthBar != null)
        {
            _discreteTimer += Time.deltaTime;
            if (_discreteTimer >= discreteStepSeconds)
            {
                _discreteTimer = 0f;
                _discreteCurrent--;
                if (_discreteCurrent < 0)
                {
                    _discreteCurrent = discreteMaxHealth;
                }
                discreteHealthBar.DisplayHealth(_discreteCurrent, discreteMaxHealth);
            }
        }
    }
}
