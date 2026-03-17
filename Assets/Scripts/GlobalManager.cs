using Hammer;
using System;
using System.IO.Ports;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }


    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public string port = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Application.targetFrameRate = 60;
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SearchPorts();
    }

    public void SearchPorts()
    {
        try
        {
            if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
            {
                port = "/dev/cu.usbmodem101";
            }

            if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
            {
                port = "/dev/ttyACM0";
            }

            if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
            {

                try
                {
                    string[] portNames = SerialPort.GetPortNames();

                    if (portNames.Length < 2)
                    {
                        Console.WriteLine("No COM ports found.");
                        return;
                    }

                    if (!string.IsNullOrEmpty(portNames[1]))
                    {
                        port = portNames[1];
                        Debug.Log(port);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error scanning COM ports: {ex.Message}");
                }

            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("Failed to find port: ");
            Debug.LogWarning(e);
        }
    }

    public static HammerBehaviour HammerBehaviour { get; private set; }

}
