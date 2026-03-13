using System;
using System.IO.Ports;
using UnityEngine;

namespace Hammer
{
    public class HammerBehaviour : MonoBehaviour
    {

        Quaternion gameRotationVector;
        Vector3 frameAcceleration;
        SerialPort stream;


        float extension;
        float extensionVelocity;
        [SerializeField] float k = 20f;
        [SerializeField] float dampingCoef = 3f;
        [SerializeField] float restLength = 1;
        [SerializeField] float maxLength = 20;
        [SerializeField] float sensitivity = 2;
        [SerializeField] float momentumDecay = 0.92f;

        private float momentum=0;

        [SerializeField] Transform pivotTransform;
        private bool portOpen = false;
        private readonly int timeoutMs = 30;

        public Rigidbody rigidBody;

        void Start()
        {
            Connect();
            rigidBody = GetComponent<Rigidbody>();
            Application.targetFrameRate = 60;
        }


        private void Connect()
        {
            try
            {
                string port = null;
                if (Application.platform.Equals(RuntimePlatform.WindowsEditor) || Application.platform.Equals(RuntimePlatform.WindowsPlayer))
                {
                    port = "COM4";
                }

                if (Application.platform.Equals(RuntimePlatform.OSXEditor) || Application.platform.Equals(RuntimePlatform.OSXPlayer))
                {
                    port = "/dev/cu.usbmodem101";
                }

                if (Application.platform.Equals(RuntimePlatform.LinuxPlayer) || Application.platform.Equals(RuntimePlatform.LinuxServer))
                {
                    port = "/dev/ttyACM0";
                }

                if (!string.IsNullOrEmpty(port))
                {
                    stream = new SerialPort(port, 19200)
                    {
                        ReadTimeout = timeoutMs
                    };
                }
                stream.DtrEnable = true;
                stream.Open();
                stream.ReadTimeout = timeoutMs;
                portOpen = true;
                Debug.Log("Connected (allegedly)");
            }
            catch (System.Exception e)
            {
                portOpen = false;
                Debug.LogWarning("Failed to connect to port: ");
                Debug.LogWarning(e);
            }
        }
        public void CalibrateHammer()
        {
            GlobalManager.Instance.CalibrationQuaternion = gameRotationVector;

        }

        void ParseStream()
        {

            string recievedData = null;

            while (stream.BytesToRead > 0)
            {
                try
                {
                    recievedData = stream.ReadLine();
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error reading data: {ex.Message}");
                    return;
                }
                
                Debug.Log(recievedData);
                string[] parsedData = recievedData.Trim().Split(':');

                if (parsedData[0] == "a")
                {
                    Vector3 acceleration = new(
                        float.Parse(parsedData[3]),
                        float.Parse(parsedData[1]),
                        float.Parse(parsedData[2])
                        );
                    // TODO change to impulse or something (persist across frames)
                    if (acceleration.magnitude > frameAcceleration.magnitude) frameAcceleration = acceleration;
                }

                if (parsedData[0] == "q")
                {
                    gameRotationVector = new Quaternion(
                                                     float.Parse(parsedData[1]),
                                                     float.Parse(parsedData[3]),
                                                     float.Parse(parsedData[2]),
                                                     float.Parse(parsedData[4]));
                }

            }

        }

        void UpdateRotation()
        {
            Quaternion incorrectRotation = Quaternion.Inverse(GlobalManager.Instance.CalibrationQuaternion) * gameRotationVector;
            Quaternion correctedRotation = new(-incorrectRotation.x, incorrectRotation.y, -incorrectRotation.z, incorrectRotation.w);
            transform.localRotation = correctedRotation;
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
            float acceleration = spring + damping + momentum*sensitivity;

            extensionVelocity += acceleration * Time.deltaTime;
            extension += extensionVelocity * Time.deltaTime;
            extension = Mathf.Clamp(extension, 0, maxLength);

            transform.position = pivotTransform.position + transform.rotation * Vector3.forward * extension;
        }

        void Update()
        {


            if (!stream.IsOpen)
            {
                Debug.Log("Port is not open for reading.");
                return;
            }

            ParseStream();
            UpdateRotation();
            UpdatePosition();
            frameAcceleration = Vector3.zero;
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
        }

    }

}
