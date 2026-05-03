using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HealthMeterUI : MonoBehaviour
{
    [SerializeField] private Image healthFillImage;
    [SerializeField] private RectTransform panelTransform;

    [Header("Pulse Settings")]
    [SerializeField] private bool pulseOnChange = true;
    [SerializeField] private float pulseScale = 1.06f;
    [SerializeField] private float pulseUpTime = 0.08f;
    [SerializeField] private float pulseDownTime = 0.12f;

    private Coroutine pulseCoroutine;

    public void DisplayFullBar()
    {
        SetHealthFraction(1.0f);
    }

    public void DisplayFractionalBar(float healthFraction)
    {
        SetHealthFraction(healthFraction);
    }

    public void DisplayBarCurMax(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0.0f)
        {
            SetHealthFraction(0.0f);
            return;
        }

        SetHealthFraction(currentHealth / maxHealth);
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

        if (pulseOnChange)
        {
            PlayPulse();
        }
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

        float elapsedTime = 0.0f;

        while (elapsedTime < pulseUpTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pulseUpTime;
            panelTransform.localScale = Vector3.Lerp(normalScale, enlargedScale, t);
            yield return null;
        }

        elapsedTime = 0.0f;

        while (elapsedTime < pulseDownTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pulseDownTime;
            panelTransform.localScale = Vector3.Lerp(enlargedScale, normalScale, t);
            yield return null;
        }

        panelTransform.localScale = normalScale;
        pulseCoroutine = null;
    }
}