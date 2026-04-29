using UnityEngine;

public class SmashSoundHelper : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip smashClip;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
        audioSource.playOnAwake = false;
    }

    void PlaySound()
    {
        if (smashClip != null)
        {
            audioSource.PlayOneShot(smashClip, 1f);
        }
    } 
}