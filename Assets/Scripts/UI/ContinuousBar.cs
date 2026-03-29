using UnityEngine;
using UnityEngine.UI;

public class ContinuousBar : MonoBehaviour
{
    public Slider slider;

    [SerializeField] private Image fillImage; // Assign in inspector for color change

    public void DisplayFullBar()
    {
        slider.value = 1;
        UpdateColor();
    }

    /*
    To display a value on the bar, use DisplayHealth().
    If using DisplayFractionalBar(), the range of the bar is [0, 1].
    */
    public void DisplayFractionalBar(float healthFraction)
    {
        if (healthFraction < 0) healthFraction = 0;
        if (healthFraction > slider.maxValue) healthFraction = slider.maxValue;
        slider.value = healthFraction;
        UpdateColor();
    }

    /* Alternatively, you can pass in the current and maximum values of the bar if easier
    and it will calculate the fraction for you. */
    public void DisplayBarCurMax(float cur, float max)
    {
        float frac = cur / max;
        DisplayFractionalBar(frac);
    }

    // If we decide on a continuous health bar, same function name should work for both for convenience.
    public void DisplayHealth(int currentLives, int maxLives)
    {
        if (maxLives <= 0)
        {
            DisplayFractionalBar(0);
            return;
        }

        float frac = Mathf.Clamp01((float)currentLives / maxLives);
        DisplayFractionalBar(frac);
    }

    private void UpdateColor()
    {
        if (fillImage != null)
        {
            float percent = slider.value / slider.maxValue;
            fillImage.color = Color.Lerp(Color.red, Color.green, percent);
        }
    }

    public void ShowBar(bool show)
    {
        gameObject.SetActive(show);
    }
}
