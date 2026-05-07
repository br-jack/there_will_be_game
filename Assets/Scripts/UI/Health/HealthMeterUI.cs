using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthMeterUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private RectTransform panelTransform;

    [Header("Pulse Settings")]
    [SerializeField] private float pulseScale = 1f;
    [SerializeField] private float pulseUpTime = 0.1f;
    [SerializeField] private float pulseDownTime = 0.1f;

    private Coroutine pulseCoroutine;

    public void DisplayFractionalBar(float healthFraction)
    {
        SetHealthFraction(healthFraction);
    }

    public void DisplayHealth(int currentLives, int maxLives)
    {
        if (maxLives <= 0)
        {
            SetHealthFraction(0.0f);
            return;
        }

        SetHealthFraction((float)currentLives / (float)maxLives);
    }

    private void SetHealthFraction(float fraction)
    {
        float clampedFraction = Mathf.Clamp01(fraction);
        healthFillImage.fillAmount = clampedFraction;
        PlayPulse();
    }

    private void PlayPulse()
    {

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        pulseCoroutine = StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        Vector3 normalScale = Vector3.one;
        Vector3 enlargedScale = new Vector3(pulseScale, pulseScale, 1.0f);

        float passedTime = 0.0f;

        while (passedTime < pulseUpTime)
        {
            passedTime += Time.deltaTime;
            float time = passedTime / pulseUpTime;
            panelTransform.localScale = Vector3.Lerp(normalScale, enlargedScale, time);
            yield return null;
        }

        passedTime = 0.0f;

        while (passedTime < pulseDownTime)
        {
            passedTime += Time.deltaTime;
            float time = passedTime / pulseDownTime;
            panelTransform.localScale = Vector3.Lerp(enlargedScale, normalScale, time);
            yield return null;
        }

        panelTransform.localScale = normalScale;
        pulseCoroutine = null;
    }
}