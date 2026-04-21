using System.Collections;
using TMPro;
using UnityEngine;

public class DeathTextUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathText;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 0.6f;
    [SerializeField] private float visibleDuration = 1.0f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void ShowDeathText()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        yield return Fade(0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(visibleDuration);

        yield return Fade(1f, 0f, fadeOutDuration);
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            SetAlpha(Mathf.Lerp(start, end, t));
            yield return null;
        }

        SetAlpha(end);
    }

    private void SetAlpha(float alpha)
    {
        Color c = deathText.color;
        c.a = alpha;
        deathText.color = c;
    }
}
