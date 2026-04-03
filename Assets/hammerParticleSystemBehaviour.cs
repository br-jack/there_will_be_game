using System;
using System.ComponentModel;
using UnityEngine;

public class hammerParticleSystemBehaviour : MonoBehaviour
{
    private float headSpeedForwards;

    private Transform _tf;
    private Vector3 posPrevFrame;

    public float trailSpeedThreshold;
    private ParticleSystem _ps;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _tf = GetComponent<Transform>();
        _ps = GetComponent<ParticleSystem>();
        posPrevFrame = _tf.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 positionChange = _tf.position - posPrevFrame;
        Vector3 velocity = positionChange/Time.deltaTime;
        Vector3 headForward = _tf.forward; //normalised forward vector from head of hammer

        //find scalar speed of hammer head in the forwards direction
        headSpeedForwards = Vector3.Magnitude(headForward * Vector3.Dot(headForward,velocity));
        Debug.Log("headSpeedForwards: "+headSpeedForwards);

        if (headSpeedForwards > trailSpeedThreshold) _ps.Play();
        //else _ps.Stop();
        
        posPrevFrame = _tf.position;
        

    }
}
