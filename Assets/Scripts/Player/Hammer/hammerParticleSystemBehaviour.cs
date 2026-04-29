using System;
using System.ComponentModel;
using UnityEngine;
using Hammer;

//this script and hammerStateEffects should likely be one script, they do the same thing. 
public class hammerParticleSystemBehaviour : MonoBehaviour
{
    public float trailSpeedThreshold;

    public float ghostSpeedThreshold;

    public uint trailLingerFrames; //probs should be done with time

    public uint ghostsLingerFrames; //also should probs be done with time
    public ParticleSystem trailsSystem;
    public ParticleSystem ghostsSystem;
    public ParticleSystemRenderer ghostsRenderer;

    private uint framesUntilTrailsDisabled;
    private uint framesUntilGhostsDisabled;

    private bool _ghostsOn;
    private bool _trailsOn;

    [SerializeField] private TargetHammer _targetHammer;


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
        if (_targetHammer.radialAcceleration > trailSpeedThreshold)
        {
            trails.enabled = true;
            framesUntilTrailsDisabled = trailLingerFrames;
        }
        else if (framesUntilTrailsDisabled == 0) trails.enabled = false;
        else framesUntilTrailsDisabled--;

        //spawn 'ghost' hammers behind the hammer if headSpeedForwards is above the threshold TODO and awesome is on
        var ghostEmission = ghostsSystem.emission;
        if (_targetHammer.radialAcceleration > ghostSpeedThreshold)
        {
            ghostEmission.enabled = true;
            framesUntilGhostsDisabled = ghostsLingerFrames;
        }
        else if (framesUntilGhostsDisabled == 0) ghostEmission.enabled = false;
        else framesUntilGhostsDisabled--;

    }
}
