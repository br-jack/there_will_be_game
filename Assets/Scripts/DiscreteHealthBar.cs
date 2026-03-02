using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DiscreteHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject healthIconPrefab; // Assign a prefab (e.g., heart image) in Inspector
    [SerializeField] private Transform iconContainer;     // Assign a UI container (e.g., empty GameObject with HorizontalLayoutGroup)

    private List<GameObject> icons = new List<GameObject>();

    public void DisplayHealth(int currentHealth, int maxHealth)
    {
        // Ensure correct number of icons
        while (icons.Count < maxHealth)
        {
            GameObject icon = Instantiate(healthIconPrefab, iconContainer);
            icons.Add(icon);
        }
        while (icons.Count > maxHealth)
        {
            Destroy(icons[icons.Count - 1]);
            icons.RemoveAt(icons.Count - 1);
        }

        // Show/hide icons based on current health
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(i < currentHealth);
        }
    }
}