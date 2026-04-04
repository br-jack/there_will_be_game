using System;
using System.ComponentModel;
using UnityEngine;

public class hammerParticleSystemBehaviour : MonoBehaviour
{
    private float headSpeedForwards;

    private Transform _tf;
    private Vector3 posPrevFrame;

    public float trailSpeedThreshold;

    public float ghostSpeedThreshold;

    public uint trailLingerFrames; //probs should be done with time
    public ParticleSystem trailsSystem;
    public ParticleSystem ghostsSystem;
    public ParticleSystemRenderer ghostsRenderer;

    private uint framesUntilTrailsDisabled;

    
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        _tf = GetComponent<Transform>();
        posPrevFrame = _tf.position;
        trailsSystem.Play();
        ghostsSystem.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
        //find scalar speed of hammer head in the forwards direction
        Vector3 positionChange = _tf.position - posPrevFrame;
        Vector3 velocityGlobal = positionChange/Time.deltaTime;
        Vector3 velocityLocal =  transform.InverseTransformDirection(velocityGlobal);
        headSpeedForwards = velocityLocal.z;
        //Debug.Log("headSpeedForwards: "+headSpeedForwards);

        //enable a trail behind the hammer if headSpeedForwards is above the threshold, with a buffer
        var trails = trailsSystem.trails;
        if (headSpeedForwards > trailSpeedThreshold) 
        {
            Debug.Log("framesUntilTrailsDisabled:" +framesUntilTrailsDisabled);
            trails.enabled = true; 
            framesUntilTrailsDisabled = trailLingerFrames;
        }
        else if (framesUntilTrailsDisabled == 0) trails.enabled = false;
        else framesUntilTrailsDisabled --;

        //spawn 'ghost' hammers behind the hammer if headSpeedForwards is above the threshold
        var ghostMain = ghostsSystem.main;
        
        var ghostEmission = ghostsSystem.emission;
        if (headSpeedForwards > ghostSpeedThreshold)
        {
            Debug.Log("hi!");
            ghostEmission.enabled = true;
            
        } else ghostEmission.enabled = false; 
        
        posPrevFrame = _tf.position;
        

    }
}
