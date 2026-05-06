using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Score
{
public class FearUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupContainer;
    [SerializeField] private Vector2 popupStartOffset = new Vector2(160f, 0f);
    [SerializeField] private float popupSpacing = 35f;
    [SerializeField] public float xScorePositionVariation;
    [SerializeField] public float yScorePositionVariation;
    
    private void Start()
    {
        if (popupContainer == null)
        {
            popupContainer = transform;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnFearChanged += UpdateScoreDisplay;
            ScoreManager.Instance.OnScoreAdded += SpawnScorePopups;
            UpdateScoreDisplay(ScoreManager.Instance.FearScore);
        }
    }
    
    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnFearChanged -= UpdateScoreDisplay;
            ScoreManager.Instance.OnScoreAdded -= SpawnScorePopups;
        }
    }
    
    // don't actually want it to display the score just want the popups
    private void UpdateScoreDisplay(int score)
    {
        scoreText.text = $"";
    }
    
    private void SpawnScorePopups(List<ScoreComponent> components)
    {   
        for (int i = 0; i < components.Count; i++)
        {
            GameObject popupObject = Instantiate(scorePopupPrefab, popupContainer);
            RectTransform popupRect = popupObject.GetComponent<RectTransform>();
            
            if (popupRect != null)
            {
                Vector2 position = popupStartOffset + new Vector2(0f, -i * popupSpacing) + new Vector2((Random.value * xScorePositionVariation),(Random.value * yScorePositionVariation));
                popupRect.anchoredPosition = position;
            }
            
            ScorePopup popup = popupObject.GetComponent<ScorePopup>();
            if (popup != null)
            {
                popup.Initialize(components[i].amount, components[i].type);
            }
        }
    }
}
}

