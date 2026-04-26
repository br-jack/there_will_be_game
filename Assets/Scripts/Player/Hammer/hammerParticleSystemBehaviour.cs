using System;
using System.ComponentModel;
using UnityEngine;
//this script and hammerStateEffects should likely be one script, they do the same thing. 

public class hammerParticleSystemBehaviour : MonoBehaviour
{
    public Hammer.hammerHead head;
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
        trailsSystem.Play();
        ghostsSystem.Play();
        
    }

    // Update is called once per frame
    void Update()
    {
        

        //enable a trail behind the hammer if headSpeedForwards is above the threshold, with a buffer
        var trails = trailsSystem.trails;
        if (head.forwardSpeed > trailSpeedThreshold) 
        {
            trails.enabled = true; 
            framesUntilTrailsDisabled = trailLingerFrames;
        }
        else if (framesUntilTrailsDisabled == 0) trails.enabled = false;
        else framesUntilTrailsDisabled --;

        //spawn 'ghost' hammers behind the hammer if headSpeedForwards is above the threshold 
        var ghostEmission = ghostsSystem.emission;
        if (head.forwardSpeed > ghostSpeedThreshold) 
        { 
            ghostEmission.enabled = true; 
            framesUntilGhostsDisabled = ghostsLingerFrames;
        }
        else if (framesUntilGhostsDisabled == 0) ghostEmission.enabled = false;
        else framesUntilGhostsDisabled --;
        
    }
}
