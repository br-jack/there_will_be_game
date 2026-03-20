using UnityEngine;

public class PlayerParticles : MonoBehaviour
{
    [SerializeField] private TrailRenderer jumpTrail;
    [SerializeField] private ParticleSystem runParticles;
    [SerializeField] private ParticleSystem jumpParticles;
    
    [SerializeField] private HorseMovement horseMovement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        jumpTrail.emitting = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
