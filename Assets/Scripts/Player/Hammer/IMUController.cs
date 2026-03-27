using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace Hammer
{
    public class IMUController : IController
    {
        private Quaternion _gameRotationVector;
        private Vector3 _frameAcceleration;

        private SerialPort _stream;

        private Thread _ioThread;
        private bool _running;
        private readonly ConcurrentQueue<string> _dataQueue = new ConcurrentQueue<string>();

        private bool _portOpen = false;
        private const int TimeoutMs = 50;

        private string _port = null;

        private void SearchPorts()
        {
            try
            {
                if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
                {
                    _port = "/dev/cu.usbmodem101";
                }

                if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
                {
                    _port = "/dev/ttyACM0";
                }

                if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
                {

                    try
                    {
                        var availablePorts = SerialPort.GetPortNames()
                            .Where(p => !p.Equals("COM1"))
                            .ToArray();

                        if (availablePorts.Length == 0)
                        {
                            Console.WriteLine("No usable COM ports found (if the hub is COM1 that's weird and also not my problem sorry).");
                            return;
                        }

                        _port = availablePorts[0];
                        Debug.Log(_port);

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

        public void Connect()
        {
            int attempts = 0;
            while (_port == null)
            {
                SearchPorts();
                attempts++;
                if (attempts == 5)
                {
                    Debug.LogWarning("Could not find port.");
                    _running = false;
                    return;
                }
            }

            try
            {
                _stream = new SerialPort(_port, 115200)
                {
                    ReadTimeout = TimeoutMs
                };

                _stream.DtrEnable = true;
                _stream.Open();
                _stream.ReadTimeout = TimeoutMs;
                _portOpen = true;
                // if youre connected but not getting any data you may have another serial monitor open for this port
                Debug.Log("Connected (allegedly)");
            }
            catch (System.Exception e)
            {
                _portOpen = false;
                Debug.LogWarning("Failed to connect to port: ");
                Debug.LogWarning(e);
            }


            _running = true;

            // Start the background I/O thread
            _ioThread = new Thread(IOThreadLoop)
            {
                IsBackground = true
            };
            _ioThread.Start();
        }

        private void IOThreadLoop()
        {
            try
            {
                while (_running)
                {
                    string recievedData = null;
                    try
                    {
                        recievedData = _stream.ReadLine();
                        _dataQueue.Enqueue(recievedData);
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
            while (_dataQueue.TryDequeue(out string data))
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
                        if (acceleration.magnitude > _frameAcceleration.magnitude) _frameAcceleration = acceleration;

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
                        Quaternion possibleQuaternion = new Quaternion(float.Parse(parsedData[2]),
                            -float.Parse(parsedData[4]),
                            float.Parse(parsedData[3]),
                            float.Parse(parsedData[1]));
                        _gameRotationVector = possibleQuaternion;

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
            if (!_stream.IsOpen)
            {
                Debug.LogWarning("Port is not open for reading.");
                return;
            }

            // this completely breaks momentum but whatever
            _frameAcceleration = new Vector3(0, 0, 0);

            ParseStream();
        }

        public Quaternion GetAttitude()
        {
            return _gameRotationVector;
        }

        public Vector3 GetAcceleration()
        {
            return _frameAcceleration;
        }

        public void Cleanup()
        {
            _running = false;
            if (_ioThread != null && _ioThread.IsAlive)
            {
                _ioThread.Join();
            }
            _ioThread = null;

            _dataQueue.Clear();

            _portOpen = false;
            _stream.Close();
            Debug.Log("Port closed");
        }
    }
}