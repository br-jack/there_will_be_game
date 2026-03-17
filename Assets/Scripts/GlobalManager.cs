using Hammer;
using System;
using System.IO.Ports;
using System.Management;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }


    public Quaternion CalibrationQuaternion = new Quaternion(1, 1, 1, 1);
    public string port = null;
    private SerialPort testStream;


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

                    if (portNames.Length == 0)
                    {
                        Console.WriteLine("No COM ports found.");
                        return;
                    }

                    foreach (string port in portNames)
                    {
                        Debug.Log(port);
                    }

                    //using (var searcher = new ManagementObjectSearcher(
                    //    "SELECT * FROM Win32_PnPEntity WHERE Name LIKE '%(COM%'"))
                    //{
                    //    foreach (ManagementObject obj in searcher.Get())
                    //    {
                    //        string friendlyName = obj["Name"]?.ToString() ?? "Unknown";
                    //        string name = ExtractName(friendlyName);
                    //        string comPort = ExtractComPort(friendlyName);
                    //        Console.WriteLine($"Port: {comPort}");
                    //        Console.WriteLine($"  Friendly Name: {name}");

                    //    }
                    //}
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

    private static string ExtractComPort(string friendlyName)
    {
        int start = friendlyName.LastIndexOf("(COM");
        if (start >= 0)
        {
            return friendlyName.Substring(start + 1, friendlyName.Length - start - 2);
        }
        return null;
    }

    private static string ExtractName(string friendlyName)
    {
        int end = friendlyName.IndexOf("(");
        if (end >= 0)
        {
            return friendlyName.Substring(0, end);
        }
        return null;
    }

}





//class Program
//{
//    static void Main()
//    {
//        
//    }


//}
