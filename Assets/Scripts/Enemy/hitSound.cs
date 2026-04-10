using UnityEngine;

public class hitSound : MonoBehaviour
{
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("man there's no audio source");
        }
        
    }
    public void PlaySFX()
    {
        
        audioSource.PlayOneShot(audioSource.clip);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
