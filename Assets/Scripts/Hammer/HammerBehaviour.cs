using System;
using System.Collections.Concurrent;
using System.IO.Ports;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        Quaternion gameRotationVector;
        Vector3 frameAcceleration;
        SerialPort stream;
        private Thread ioThread;
        private bool running;
        private ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();

        [SerializeField] float extension;
        float extensionVelocity;
        [SerializeField] float k = 20f;
        [SerializeField] float dampingCoef = 3f;
        [SerializeField] float restLength = 1;
        [SerializeField] float maxLength = 20;
        [SerializeField] float sensitivity = 2;
        [SerializeField] float momentumDecay = 0.92f;

        private float momentum = 0;

        [SerializeField] Transform pivotTransform;
        private bool portOpen = false;
        private readonly int timeoutMs = 50;

        public Rigidbody rigidBody;

        void Start()
        {
            int attempts = 0;
            while (GlobalManager.Instance.port.IsUnityNull())
            {
                GlobalManager.Instance.SearchPorts();
                attempts++;
                if (attempts == 5)
                {
                    Debug.LogWarning("Could not find port.");
                    running = false;
                    return;
                }
            }

            Connect();
            rigidBody = GetComponent<Rigidbody>();

            running = true;

            // Start the background I/O thread
            ioThread = new Thread(IOThreadLoop)
            {
                IsBackground = true
            };
            ioThread.Start();
        }


        private void Connect()
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
                        // Debug.LogWarning($"Error reading data: {ex.Message}");
                    }

                }
            }
            catch (Exception ex)
            {
                // Debug.LogError($"[IO Thread] Error: {ex.Message}");
            }
        }

        public void CalibrateHammer()
        {
            GlobalManager.Instance.CalibrationQuaternion = Quaternion.Inverse(gameRotationVector);

        }

        void ParseStream()
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

                if (parsedData[0] == "q")
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



        void UpdateRotation()
        {
            Quaternion newRotation = gameRotationVector * GlobalManager.Instance.CalibrationQuaternion;
            float diff = Quaternion.Angle(newRotation, gameRotationVector);
            if (diff < 160.0f)
            {
                transform.localRotation = newRotation;
            }

        }

        void UpdatePosition()
        {
            Vector3 worldForward = transform.rotation * Vector3.forward;
            float radialAcceleration = Vector3.Dot(frameAcceleration, worldForward);
            float force = Mathf.Abs(radialAcceleration) < 0.1f ? 0f : radialAcceleration;

            momentum += force * Time.deltaTime;
            momentum *= momentumDecay;


            float spring = -k * (extension - restLength);
            float damping = -dampingCoef * extensionVelocity;
            float acceleration = spring + damping + momentum * sensitivity;

            extensionVelocity += acceleration * Time.deltaTime;
            extension += extensionVelocity * Time.deltaTime;
            extension = Mathf.Clamp(extension, 0, maxLength);

            transform.position = pivotTransform.position + transform.rotation * Vector3.forward * extension;
        }

        void Update()
        {

            if (!stream.IsOpen)
            {
                Debug.LogWarning("Port is not open for reading.");
                return;
            }

            ParseStream();
            UpdateRotation();
            UpdatePosition();

            // this completely breaks momentum but whatever
            frameAcceleration = new Vector3(0, 0, 0);
        }

        public void OnCollisionEnter(Collision collision)
        {

            if (collision.gameObject.CompareTag("Enemy"))
            {
                Destroy(collision.gameObject);
            }
        }

        void OnDisable()
        {
            portOpen = false;
            stream.Close();
            Debug.Log("Port closed");
            
            running = false;
            if (ioThread != null && ioThread.IsAlive)
            {
                ioThread.Join();
            }
        }
    }

}
