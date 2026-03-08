using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private Transform popupContainer;
    [SerializeField] private Vector2 popupStartOffset = new Vector2(150f, 0f);
    [SerializeField] private float popupSpacing = 30f;
    
    private void Start()
    {
        if (popupContainer == null)
        {
            popupContainer = transform;
        }
        
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            ScoreManager.Instance.OnScoreAdded += SpawnScorePopups;
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }
    
    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            ScoreManager.Instance.OnScoreAdded -= SpawnScorePopups;
        }
    }
    
    private void UpdateScoreDisplay(int score)
    {
        scoreText.text = $"FEAR Score: {score}";
    }
    
    private void SpawnScorePopups(List<ScoreComponent> components)
    {
        if (scorePopupPrefab == null)
        {
            return;
        }
        
        for (int i = 0; i < components.Count; i++)
        {
            GameObject popupObj = Instantiate(scorePopupPrefab, popupContainer);
            RectTransform popupRect = popupObj.GetComponent<RectTransform>();
            
            if (popupRect != null)
            {
                Vector2 position = popupStartOffset + new Vector2(0f, -i * popupSpacing);
                popupRect.anchoredPosition = position;
            }
            
            ScorePopup popup = popupObj.GetComponent<ScorePopup>();
            if (popup != null)
            {
                popup.Initialize(components[i].amount, components[i].type);
            }
        }
    }
}
