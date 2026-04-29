using UnityEngine;

/*
This is purely for TESTING the continuos bar.
In the inspector, check the bar/bars that you want to test, then you can watch:
- The continuous bar cycle from 1 to 0 and back to 1

When changing the design of either, this will let you see what it looks like at every bar percentage.
*/
public class BarsTester : MonoBehaviour
{
    [Header("References")]
    public HealthMeterUI healthBar;

    [Header("Toggle to True to Cycle Through The Bars' States (TEST PURPOSES)")]
    public bool runContinuousTest = true;

    [Header("Continuous Settings")]
    [Tooltip("Seconds for a full ramp from full to empty and back.")]
    private float continuousCycleSeconds = 4f;

    private void Update()
    {
        if (runContinuousTest && healthBar != null)
        {
            // Ping-pong between full and empty over the cycle time.
            float t = Mathf.PingPong(Time.time, continuousCycleSeconds) / continuousCycleSeconds;
            float fraction = 1f - t;
            healthBar.DisplayFractionalBar(fraction);
        }


    }
}
