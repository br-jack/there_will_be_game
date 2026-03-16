using UnityEngine;
using TMPro;
using System.Collections;

public class ScorePopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private float floatSpeed = 50f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float fadeStartTime = 0.5f;
    
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private float timer = 0f;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    public void Initialize(int amount, ScoreType type)
    {
        if (popupText == null)
        {
            popupText = GetComponent<TextMeshProUGUI>();
        }
        
        popupText.text = $"+{amount}";
        popupText.color = GetColorForType(type);
        
        StartCoroutine(AnimatePopup());
    }
    
    private Color GetColorForType(ScoreType type)
    {
        switch (type)
        {
            case ScoreType.Base:
                return new Color(1f, 1f, 1f); // White
            case ScoreType.Speed:
                return new Color(1f, 0.5f, 0f); // Orange
            case ScoreType.LowHealth:
                return new Color(1f, 0f, 0f); // Red
            case ScoreType.Air:
                return new Color(0.3f, 0.8f, 1f); // Light blue
            case ScoreType.ShieldBypass:
                return new Color(1f, 0.84f, 0f); // Gold
            default:
                return Color.white;
        }
    }
    
    private IEnumerator AnimatePopup()
    {
        Vector3 startPos = rectTransform.anchoredPosition;

        // Makes the numbers float up and fade away after a bit
        while (timer < lifetime)
        {
            timer += Time.deltaTime;
            
            rectTransform.anchoredPosition = startPos + Vector3.up * (floatSpeed * timer);
            
            if (timer > fadeStartTime)
            {
                float fadeProgress = (timer - fadeStartTime) / (lifetime - fadeStartTime);
                canvasGroup.alpha = 1f - fadeProgress;
            }
            
            // Using yield forces the loop to only run once per frame
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
