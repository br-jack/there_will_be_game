using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroFadeText : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introOverlay;
    [SerializeField] private Image dimBackground;
    [SerializeField] private TextMeshProUGUI introText;

    [Header("Message")]
    [SerializeField] private string message = "A god is helping you achieve your task";
    [SerializeField] private float letterDelay = 0.07f;
    [SerializeField] private float holdTime = 2f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float maxBackgroundAlpha = 0.75f;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject gameplayUI;

    [Header("Tutorial Prompt UI")]
    [SerializeField] private GameObject tutorialPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string firstPromptMessage = "Swing your hammer 3 times";

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // At the start of the intro hide the gameplay UI and tutorial prompts
        gameplayUI.SetActive(false);
        tutorialPromptUI.SetActive(false);

        introOverlay.SetActive(true);

        // Start with invisible background and empty text
        Color background = dimBackground.color;
        background.a = 0f;
        dimBackground.color = background;

        introText.text = "";
        Color textColour = introText.color;
        textColour.a = 1f;
        introText.color = textColour;

        // Fade in dark background
        yield return StartCoroutine(FadeBackground(0f, maxBackgroundAlpha, fadeInTime));

        // Type text letter by letter
        yield return StartCoroutine(TypeText(message));

        // Hold for a moment
        yield return new WaitForSeconds(holdTime);

        // Fade everything back out
        yield return StartCoroutine(FadeOutOverlay());

        introOverlay.SetActive(false);

        tutorialPromptUI.SetActive(true);
        promptText.text = firstPromptMessage;
    }

    private IEnumerator TypeText(string fullMessage)
    {
        introText.text = "";

        for (int i = 0; i < fullMessage.Length; i++)
        {
            introText.text += fullMessage[i];
            yield return new WaitForSeconds(letterDelay);
        }
    }

    private IEnumerator FadeBackground(float startAlpha, float endAlpha, float duration)
    {

        float elapsed = 0f;
        Color background = dimBackground.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            background.a = Mathf.Lerp(startAlpha, endAlpha, t);
            dimBackground.color = background;

            yield return null;
        }

        background.a = endAlpha;
        dimBackground.color = background;
    }

    private IEnumerator FadeOutOverlay()
    {
        float elapsed = 0f;

        Color startBackground = dimBackground.color;
        Color startText = introText.color;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / fadeOutTime);

            Color background = startBackground;
            background.a = Mathf.Lerp(startBackground.a, 0f, percent);
            dimBackground.color = background;

            Color textColour = startText;
            textColour.a = Mathf.Lerp(startText.a, 0f, percent);
            introText.color = textColour;

            yield return null;
        }

        Color finalBackground = dimBackground.color;
        finalBackground.a = 0f;
        dimBackground.color = finalBackground;

        Color finalTextColour = introText.color;
        finalTextColour.a = 0f;
        introText.color = finalTextColour;
    }
}
