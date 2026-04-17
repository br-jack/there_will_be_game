using UnityEngine;

public class footballOnKickEffects : MonoBehaviour
{
    public Transform _footballTransform;
    private Transform _tf;
    private ParticleSystem _particles;
    public void PlayOnKickEffects()
    {
        _particles.Play();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _particles = GetComponent<ParticleSystem>();
        _tf = GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        _tf.position = _footballTransform.position;
    }
}
