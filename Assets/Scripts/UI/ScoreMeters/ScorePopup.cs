using UnityEngine;
using TMPro;
using System.Collections;

namespace Score
{
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
        
        string popupExplanation = GetExplanationForType(type);
        popupText.text = $"+{amount} {popupExplanation}";
        popupText.color = GetColorForType(type);
        
        StartCoroutine(AnimatePopup());
    }
    
    private string GetExplanationForType(ScoreType type) {
        switch (type) {
            case ScoreType.Base:
                return "Silenced!"; // White
            //case ScoreType.Speed:
                //return ""; // Orange
            case ScoreType.atATrot:
                return "At a Trot"; 
            case ScoreType.atACanter:
                return "At a Canter"; 
            case ScoreType.atAGallop:
                return "At a Gallop"; 
            case ScoreType.LowHealth:
                return "Against all odds";
            case ScoreType.Air:
                return "#airtime";
            case ScoreType.ShieldBypass:
                return "Past a shield"; // Gold
            case ScoreType.OnFire:
                return "Enflamed!"; // Bright red
            case ScoreType.Building: 
                return "Renovation"; //brown
            default:
                return "Misc. Bonus";
        }
    }
    
    private Color GetColorForType(ScoreType type)
    {
        switch (type)
        {
            case ScoreType.Base:
                return new Color(1f, 1f, 1f); // White
            //case ScoreType.Speed:
                //return new Color(1f, 0.5f, 0f); // Orange
            case ScoreType.atATrot:
                return Color.green; 
            case ScoreType.atACanter:
                return Color.yellow; 
            case ScoreType.atAGallop:
                return new Color(1f, 0.5f, 0f); // Orange; 
            case ScoreType.LowHealth:
                return new Color(1f, 0f, 0f); // Red
            case ScoreType.Air:
                return new Color(0.3f, 0.8f, 1f); // Light blue
            case ScoreType.ShieldBypass:
                return new Color(1f, 0.84f, 0f); // Gold
            case ScoreType.OnFire:
                return new Color(1f, 0.2f, 0.2f); // Bright red
            case ScoreType.Building: 
                return new Color(137,87,41); //brown
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

}
