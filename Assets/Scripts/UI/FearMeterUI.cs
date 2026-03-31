using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FearMeterUI : MonoBehaviour
{
    public RectTransform barFillTransform;
    public Image barFillImage;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI titleText;
    public Image fearIcon;
    public RectTransform panelTransform;

    public int maxFear = 300;
    public float fullBarWidth = 280.0f;

    public Color lowFearColor = new Color(1.0f, 0.72f, 0.15f, 1.0f);
    public Color midFearColor = new Color(1.0f, 0.45f, 0.10f, 1.0f);
    public Color highFearColor = new Color(0.90f, 0.15f, 0.15f, 1.0f);

    public float pulseScale = 1.06f;
    public float pulseUpTime = 0.08f;
    public float pulseDownTime = 0.12f;

    private Coroutine pulseCoroutine;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            UpdateFearUI(ScoreManager.Instance.CurrentScore);
        }
        else
        {
            Debug.LogWarning("FearMeterUI: No ScoreManager instance found in scene.");
            UpdateFearUI(0);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        UpdateFearUI(newScore);
        PlayPulse();
    }

    private void UpdateFearUI(int currentFear)
    {
        float normalizedFear = 0.0f;

        if (maxFear > 0)
        {
            normalizedFear = Mathf.Clamp01((float)currentFear / (float)maxFear);
        }

        float newWidth = fullBarWidth * normalizedFear;
        Color currentColor = GetFearColor(normalizedFear);

        if (barFillTransform != null)
        {
            barFillTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        }

        if (barFillImage != null)
        {
            barFillImage.color = currentColor;
        }

        if (scoreText != null)
        {
            scoreText.text = currentFear.ToString();
        }

        if (titleText != null)
        {
            titleText.color = currentColor;
        }

        if (fearIcon != null)
        {
            fearIcon.color = currentColor;
        }
    }

    private Color GetFearColor(float normalizedFear)
    {
        if (normalizedFear < 0.4f)
        {
            return lowFearColor;
        }
        else if (normalizedFear < 0.75f)
        {
            return midFearColor;
        }
        else
        {
            return highFearColor;
        }
    }

    private void PlayPulse()
    {
        if (panelTransform == null)
        {
            return;
        }

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