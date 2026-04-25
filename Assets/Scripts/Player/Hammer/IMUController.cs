using System;
using System.Collections.Concurrent;
using System.IO.Ports;
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
        private volatile bool _running;
        private readonly ConcurrentQueue<string> sendQueue = new ConcurrentQueue<string>();
        private readonly ConcurrentQueue<string> recvQueue = new ConcurrentQueue<string>();
        
        private IMUConfig configSO;

        public IMUController(IMUConfig config)
        {
            configSO = config;
        }
        
        private string SearchPorts()
        {
            try
            {
                if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
                {
                    return "/dev/cu.usbmodem101";
                }

                if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
                {
                    return "/dev/ttyACM0";
                }

                if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
                {

                    try
                    {
                        string[] availablePorts = SerialPort.GetPortNames();

                        if (availablePorts.Length == 0)
                        {
                            Console.WriteLine("No usable COM ports found (if the hub is COM1 that's weird and also not my problem sorry).");
                            return null;
                        }

                        foreach (string possiblePort in availablePorts)
                        {
                            Debug.Log("Trying port " + possiblePort);
                            
                            SerialPort testSerial = new SerialPort(possiblePort, 115200)
                            {
                                DtrEnable = true,
                                ReadTimeout = configSO.readTimeout * 2
                            };
                            testSerial.Open();
                            
                            //wait to ensure IMU data gets received
                            Thread.Sleep(100);
                            
                            String output = testSerial.ReadExisting();
                            testSerial.Close();
                            
                            if (output.Contains("q:") || output.Contains("a:") || output.Contains("info:"))
                            {
                                //(hub sending) IMU data found
                                Debug.Log("Hub found on port " + possiblePort);
                                return possiblePort;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error scanning COM ports: {ex.Message}");
                    }
                }
            }
            catch// (System.Exception e)
            {
                // Debug.LogWarning("Failed to find port: ");
                // Debug.LogWarning(e);
            }

            return null;
        }

        //Returns if connected successfully or not
        private bool ConnectToPort(string port)
        {
            if (port == null)
            {
                return false;
            }
            
            try
            {
                _stream = new SerialPort(port, 115200)
                {
                    ReadTimeout = configSO.readTimeout,
                    WriteTimeout = configSO.writeTimeout,
                    NewLine = "\n",
                    //needed to connect to arduino
                    DtrEnable = true,
                };

                if (_stream != null)
                {
                    _stream.Open();
                    if (_stream.IsOpen)
                    {
                        // if youre connected but not getting any data you may have another serial monitor open for this port
                        Debug.Log("Connected (allegedly)");
                        
                        return true;
                    }
                }
            }
            catch// (System.Exception e)
            {
                // Debug.LogWarning("Failed to connect to port: ");
                // Debug.LogWarning(e);
            }

            return false;
        }

        public void Connect()
        {
            _running = true;

            // Start the background I/O thread
            if (_ioThread == null)
            {
                _ioThread = new Thread(IOThreadLoop)
                {
                    IsBackground = true
                };
                _ioThread.Start();
            }
        }

        private void IOThreadLoop()
        {
            bool portOpen = false;
            
            try
            {
                while (_running && !portOpen)
                {
                    string port = SearchPorts();
                    if (port != null)
                    {
                        portOpen = ConnectToPort(port);
                    }

                    if (!portOpen)
                    {
                        Thread.Sleep(1000);
                    }
                }
                
                while (_running)
                {
                    try
                    {
                        if (!sendQueue.IsEmpty)
                        {
                            sendQueue.TryDequeue(out string dataToSend);

                            _stream.WriteLine(dataToSend);
                        }
                        
                        string receivedData = _stream.ReadLine();
                        recvQueue.Enqueue(receivedData);
                    }
                    catch// (Exception ex)
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
            while (recvQueue.TryDequeue(out string data))
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
                        //Serial breadboard
                        /*Quaternion possibleQuaternion = new Quaternion(float.Parse(parsedData[4]),
                            float.Parse(parsedData[2]),
                            float.Parse(parsedData[3]),
                            float.Parse(parsedData[1]));*/

                        //Physical hammer w,x,y,z
                        Quaternion possibleQuaternion = new Quaternion(float.Parse(parsedData[3]),
                            float.Parse(parsedData[4]),
                            float.Parse(parsedData[2]),
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
                // _ioThread.Interrupt();
                _ioThread.Join();
            }
            _ioThread = null;

            recvQueue.Clear();
            sendQueue.Clear();

            _stream?.Close();
            _stream = null;
            
            Debug.Log("Port closed");
        }

        private enum RumbleMode
        {
            Off,
            Constant,
            RampUp,
            RampDown,
        }

        private char GetRumbleModeByte(RumbleMode mode)
        {
            switch (mode)
            {
                case RumbleMode.Off:
                    return 'O';
                case RumbleMode.Constant:
                    return 'C';
                case RumbleMode.RampUp:
                    return 'U';
                case RumbleMode.RampDown:
                    return 'D';
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, "Rumble mode not handled!");
            }
        }

        private void SendRumbleRequest(
            RumbleMode mode,
            bool flipMotorDirection,
            int msDuration,
            int startStrength,
            int endStrength,
            int fadeRate,
            int fadeInterval
        )
        {
            Debug.Assert(msDuration > 0);
            Debug.Assert(startStrength > 0);
            Debug.Assert(endStrength > 0);
            Debug.Assert(fadeRate > 0);
            Debug.Assert(fadeInterval > 0);
            
            Debug.Log("Sending Rumble Request");
            sendQueue.Enqueue(
                $"R{GetRumbleModeByte(mode)}{Convert.ToInt32(flipMotorDirection)}{msDuration};{startStrength};{endStrength};{fadeRate};{fadeInterval}"
            );
        }

        public void ConstantRumble(int msDuration, int strength)
        {
            SendRumbleRequest(RumbleMode.Constant, false, msDuration, strength, strength, 0, 30);
        }

        public void GradientRumble(int totalDuration, int startStrength, int endStrength, int fadeDuration)
        {
            RumbleMode mode = (startStrength <= endStrength) ? RumbleMode.RampUp : RumbleMode.RampDown;

            int strengthDifference = Math.Abs(endStrength - startStrength);

            //TODO integer division here could be better handled, not perfectly accurate
            int numSteps = fadeDuration / configSO.rumbleFadeInterval;
            
            int fadeRate = strengthDifference / numSteps;
            
            SendRumbleRequest(mode, false, totalDuration, startStrength, endStrength, fadeRate, configSO.rumbleFadeInterval);
        }

        public void Rumble(int msDuration)
        {
            if (msDuration < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(msDuration), "Rumble duration must be non-negative");
            }
            
            Debug.Log("Sending Rumble Request");
            ConstantRumble(msDuration, 255);
        }
    }
}