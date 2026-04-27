using UnityEngine;

public class FootballAudio : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip kickClip;
    
    [Header("Settings")]
    [SerializeField] private float minForce = 2f; 
    [SerializeField] private float soundCooldown = 0.15f;
    
    private float _lastTimePlayed;

    private void OnCollisionEnter(Collision collision)
    {
        if (Time.time - _lastTimePlayed < soundCooldown) return;

        if (collision.relativeVelocity.magnitude > minForce)
        {
            float volume = Mathf.Clamp01(collision.relativeVelocity.magnitude / 10f);
            sfxSource.PlayOneShot(kickClip, volume);
            
            _lastTimePlayed = Time.time;
        }
    }
}