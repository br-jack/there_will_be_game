using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToMain : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string sceneToLoad = "MainScene";
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag(playerTag) && triggerCollider.enabled)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    public void EnableDoor()
    {
        triggerCollider.enabled = true;
    }
}
