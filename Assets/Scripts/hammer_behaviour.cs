using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using WiimoteApi;

public class hammer_behaviour : MonoBehaviour
{

    public WiimoteController wiimoteController;
    
    //Hammer should start flat with Pitch = 0
    private readonly Quaternion _startingRotation = Quaternion.Euler(new Vector3(270,0,0));

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        wiimoteController.RotateObject(transform);
        
        //Unity Remote
        //transform.rotation = Quaternion.Inverse(Input.gyro.attitude * _startingRotation);


        /*
        also doesn't work because of input system
        if (Input.GetKeyDown(KeyCode.Space)) {
            print("Looking for connected controllers");
            ConnectController();
            }
        */

    }
}
