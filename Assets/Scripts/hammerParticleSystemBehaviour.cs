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
    
    //probs can use this to properly enable and disable particle system
    private ParticleSystemRenderer _ps_rend;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _tf = GetComponent<Transform>();
        _ps = GetComponent<ParticleSystem>();
        _ps_rend = GetComponent<ParticleSystemRenderer>();
        _ps.Play();
        posPrevFrame = _tf.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 positionChange = _tf.position - posPrevFrame;
        Vector3 velocityGlobal = positionChange/Time.deltaTime;
        Vector3 velocityLocal =  transform.InverseTransformDirection(velocityGlobal);
        headSpeedForwards = velocityLocal.z;
        

        //find scalar speed of hammer head in the forwards direction
        
        Debug.Log("headSpeedForwards: "+headSpeedForwards);

        if (headSpeedForwards > trailSpeedThreshold) _ps.Play();
        else _ps.Stop();
        
        posPrevFrame = _tf.position;
        

    }
}
