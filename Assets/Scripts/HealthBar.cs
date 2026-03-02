using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Image fillImage; // Assign in inspector for color change

    public void DisplayMaxHealth(int maxHealth)
    {
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        UpdateColor();
    }

    public void DisplayHealth(int health)
    {
        slider.value = Mathf.Clamp(health, 0, slider.maxValue);
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
