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

        var trails = _ps.trails;

        if (headSpeedForwards > trailSpeedThreshold) trails.enabled = true;
        else trails.enabled = false;
        
        posPrevFrame = _tf.position;
        

    }
}
