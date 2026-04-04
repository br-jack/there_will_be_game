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

    public uint ghostsLingerFrames; //also should probs be done with time
    public ParticleSystem trailsSystem;
    public ParticleSystem ghostsSystem;
    public ParticleSystemRenderer ghostsRenderer;

    private uint framesUntilTrailsDisabled;

    private uint framesUntilGhostsDisabled;

    
    

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
            trails.enabled = true; 
            framesUntilTrailsDisabled = trailLingerFrames;
        }
        else if (framesUntilTrailsDisabled == 0) trails.enabled = false;
        else framesUntilTrailsDisabled --;

        //spawn 'ghost' hammers behind the hammer if headSpeedForwards is above the threshold 
        var ghostEmission = ghostsSystem.emission;
        if (headSpeedForwards > ghostSpeedThreshold) 
        { 
            ghostEmission.enabled = true; 
            framesUntilGhostsDisabled = ghostsLingerFrames;
        }
        else if (framesUntilGhostsDisabled == 0) ghostEmission.enabled = false;
        else framesUntilGhostsDisabled --;
        
        posPrevFrame = _tf.position; //keep track of previous frame to calculate velocities
    }
}
