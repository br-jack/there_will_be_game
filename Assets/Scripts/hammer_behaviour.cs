using System;
using UnityEngine;

public class hammer_behaviour : MonoBehaviour
{
    static public Quaternion startingRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       //Hammer should be flat with Pitch = 0
       startingRotation = Quaternion.Euler(new Vector3(270,0,0));

        print("Tap device screen to enable gyroscope!");
    }

    // Update is called once per frame
    void Update()
    {   
        //start() sadly runs after input is connected,
        //so I put this here. 
        //Sort enabling of gyroscopes later with input manager object probably
        if (Input.touchCount > 0) Input.gyro.enabled = true;
        

        //Debug.Log(Input.gyro.attitude);
        transform.rotation = Quaternion.Inverse(Input.gyro.attitude * startingRotation);
    }
}
