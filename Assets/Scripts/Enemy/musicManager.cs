using UnityEngine;

public class musicManager : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] private int startTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
        audioSource.Pause();
    }
    public void PlayMusic()
    {
        audioSource.Play();
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
