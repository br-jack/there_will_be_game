using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageVignetteFlash : MonoBehaviour
{
    [SerializeField] private Image damageVignette;
    [SerializeField] private float flashAlpha = 0.75f;
    [SerializeField] private float fadeSpeed = 8f;

    private Coroutine flashRoutine;

    public void Flash()
    {
        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        Color color = damageVignette.color;
        color.a = flashAlpha;
        damageVignette.color = color;

        while (damageVignette.color.a > 0.01f)
        {
            color = damageVignette.color;
            color.a = Mathf.Lerp(color.a, 0f, fadeSpeed * Time.deltaTime);
            damageVignette.color = color;
            yield return null;
        }

        color = damageVignette.color;
        color.a = 0f;
        damageVignette.color = color;

        flashRoutine = null;
    }
}