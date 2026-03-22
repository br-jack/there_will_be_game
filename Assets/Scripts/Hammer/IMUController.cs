using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using UnityEngine;

namespace Hammer
{
    public class IMUController : IController
    {
        private Quaternion gameRotationVector;
        private Vector3 frameAcceleration;
        
        private SerialPort stream;
        
        private Thread ioThread;
        private bool running;
        private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();
        
        private bool portOpen = false;
        private readonly int timeoutMs = 50;
        
        public void Connect()
        {
            try
            {
                stream = new SerialPort(GlobalManager.Instance.port, 115200)
                {
                    ReadTimeout = timeoutMs
                };

                stream.DtrEnable = true;
                stream.Open();
                stream.ReadTimeout = timeoutMs;
                portOpen = true;
                // if youre connected but not getting any data you may have another serial monitor open for this port
                Debug.Log("Connected (allegedly)");
            }
            catch (System.Exception e)
            {
                portOpen = false;
                Debug.LogWarning("Failed to connect to port: ");
                Debug.LogWarning(e);
            }
        }
        
        private void IOThreadLoop()
        {
            try
            {
                while (running)
                {
                    string recievedData = null;
                    try
                    {
                        recievedData = stream.ReadLine();
                        dataQueue.Enqueue(recievedData);
                    }
                    catch (Exception ex)
                    {
                        //Seems to cause a memory leak, so only enable this when debugging Bluetooth
                        // Debug.LogWarning($"Error reading data: {ex.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[IO Thread] Error: {ex.Message}");
            }
        }

        private void ParseStream()
        {
            while (dataQueue.TryDequeue(out string data))
            {
                Debug.Log($"[Main Thread] Received: {data}");
                string[] parsedData = data.Trim().Split(':');


                if (parsedData[0] == "a")
                {
                    try
                    {
                        Vector3 acceleration = new(
                                                float.Parse(parsedData[3]),
                                                float.Parse(parsedData[1]),
                                                float.Parse(parsedData[2])
                                                );
                        // TODO change to impulse or something (persist across frames)
                        if (acceleration.magnitude > frameAcceleration.magnitude) frameAcceleration = acceleration;

                    }
                    catch
                    {
                        Debug.LogWarning("Incorrect acceleration format.");
                    }

                }

                else if (parsedData[0] == "q")
                {
                    try
                    {
                        //Quaternion possibleQuaternion = new Quaternion(-float.Parse(parsedData[3]),
                        //    -float.Parse(parsedData[4]),
                        //    float.Parse(parsedData[2]),
                        //    float.Parse(parsedData[1]));
                        Quaternion possibleQuaternion = new Quaternion(float.Parse(parsedData[2]),
                            -float.Parse(parsedData[4]),
                            float.Parse(parsedData[3]),
                            float.Parse(parsedData[1]));
                        gameRotationVector = possibleQuaternion;

                    }
                    catch
                    {
                        Debug.LogWarning("Incorrect quaternion format.");

                    }


                }
            }

        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetAttitude()
        {
            throw new System.NotImplementedException();
        }

        public Vector3 GetAcceleration()
        {
            throw new System.NotImplementedException();
        }

        public void Cleanup()
        {
            throw new System.NotImplementedException();
        }
    }
}