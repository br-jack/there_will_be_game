using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/*
Responsible for displaying the current player health.
When "Testing the discrete bar" is NOT checked, this is connected to the player's health.
*/

public class DiscreteHealthBar : MonoBehaviour
{
    [SerializeField] private GameObject healthIconPrefab;
    [SerializeField] private Transform iconContainer;   

    private List<GameObject> icons = new List<GameObject>();

    private void Awake()
    {
        if (iconContainer == null) iconContainer = transform;
        
        // Record any existing health icons on the scene (prevents extra icons being added)
        icons.Clear();
        for (int i = 0; i < iconContainer.childCount; i++)
        {
            icons.Add(iconContainer.GetChild(i).gameObject);
        }
    }

    public void DisplayHealth(int cur, int max) // pass in current health and maximum possible health as parameters
    {
        // Ensure correct number of icons
        while (icons.Count < max)
        {
            GameObject icon = Instantiate(healthIconPrefab, iconContainer);
            icons.Add(icon);
        }

        // Ensures the number of icons NEVER exceeds the maximum by repeatedly deleting the last icon
        while (icons.Count > max)
        {
            Destroy(icons[icons.Count - 1]);
            icons.RemoveAt(icons.Count - 1);
        }

        // Show/hide icons based on current health
        for (int i = 0; i < icons.Count; i++)
        {
            icons[i].SetActive(i < cur);
        }
    }
}
