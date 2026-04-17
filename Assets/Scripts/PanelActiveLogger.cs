using UnityEngine;

public class PanelActiveLogger : MonoBehaviour
{
    private void OnEnable()
    {
        Debug.Log($"[Panel] '{name}' ENABLED at frame {Time.frameCount}\n{System.Environment.StackTrace}");
    }

    private void OnDisable()
    {
        Debug.Log($"[Panel] '{name}' DISABLED at frame {Time.frameCount}\n{System.Environment.StackTrace}");
    }
}
