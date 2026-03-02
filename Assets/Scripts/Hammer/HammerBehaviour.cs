using System;
using System.Globalization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using WiimoteApi;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

namespace Hammer
{
    //probs should set up a mechanism for calibrating the accelerometer. 
    // This will need the game to take the user through a short process.  
    //currently just uses whatever calibration values are in there. 

    public class HammerBehaviour : MonoBehaviour
    {

        public enum InputDevice
        {
            WIIMOTE,
            PHONE
        }
        
        public InputDevice InputDeviceType { get; private set; }

        private IRotatable _input;
        
        //Hammer should start flat with Pitch = 0
        public Quaternion StartingRotation { get; private set; }

        private Quaternion _attitude;

        //Called once before start when the game starts
        private void Awake()
        {
            StartingRotation = transform.rotation;

            if (WiimoteController.IsAvailable())
            {
                _input = new WiimoteController();
                InputDeviceType = InputDevice.WIIMOTE;
            }
            if (PhoneController.IsAvailable())
            {
                _input = new PhoneController();
                InputDeviceType = InputDevice.PHONE;
            }
            
            _input.Connect();
        }

        // Update is called once per frame
        private void Update()
        {
            if (InputDeviceType == InputDevice.PHONE)
            {
                //start() sadly runs after input is connected,
                //so I put this here. 
                //Sort enabling of gyroscopes later with input manager object probably
                if (Input.touchCount > 0)
                {
                    _input.Connect();
                }
            }
            
            transform.Rotate(_input.GetRotationOffset(), Space.Self);
        }

        public void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }

        void OnApplicationQuit()
        {
            _input.Cleanup();
        }
    }

}
