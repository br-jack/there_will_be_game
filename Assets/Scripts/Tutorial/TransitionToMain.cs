using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionToMain : MonoBehaviour
{
    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("Player") && triggerCollider.enabled)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    public void EnableDoor()
    {
        triggerCollider.enabled = true;
    }
}
