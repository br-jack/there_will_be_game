using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToMain : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string sceneToLoad = "MainScene";

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger entered by: " + other.gameObject.name);
        if (other.CompareTag(playerTag))
        {
            Debug.Log("Player entered trigger, transitioning to main scene.");
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
