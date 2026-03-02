using UnityEngine;
using UnityEngine.UI;

/*
To display a value on the healthbar, use DisplayHealth().
The range of the healthbar is [0, 1].
So if you want 4 lives, display {0, 0.25, 0.50, 0.75, 1} for example.
*/
public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Image fillImage; // Assign in inspector for color change

    public void DisplayMaxHealth()
    {
        slider.value = 1;
        UpdateColor();
    }

    public void DisplayHealth(float healthFraction)
    {
        if (healthFraction < 0) healthFraction = 0;
        if (healthFraction > slider.maxValue) healthFraction = slider.maxValue;
        slider.value = healthFraction;
        UpdateColor();
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
