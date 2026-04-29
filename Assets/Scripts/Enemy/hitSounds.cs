using UnityEngine;

public class hitSounds : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] public AudioClip hitSound;
    [SerializeField] public AudioClip hitSound1;
    [SerializeField] public AudioClip hitSound2;
    [SerializeField] public AudioClip hitSound3;
    [SerializeField] public AudioClip hitSound4;
    [SerializeField] private AudioClip[] hitSFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        hitSFX = new AudioClip[]
        {
            hitSound,
            hitSound1,
            hitSound2,
            hitSound3,
            hitSound4
        };
    }
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
        AudioClip clip = hitSFX[Random.Range(0, hitSFX.Length)];
        if (clip != null)
        {
            audioSource.clip = clip;
        }
        audioSource.PlayOneShot(audioSource.clip);
    }
}
