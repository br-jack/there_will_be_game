using UnityEngine;

public class musicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private int startTime;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("man there's no audio source: MUSIC MANAGER");
        }
        else
        {
            audioSource.time = startTime;
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
        }
    }
    public void PlayMusic()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
    public void PlaySFX()
    {
        if (audioSource != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
