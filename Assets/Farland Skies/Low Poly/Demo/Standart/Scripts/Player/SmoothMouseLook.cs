/**
 * This is an enhanced version of the FPSWalker from UnifyWiki:
 * http://wiki.unity3d.com/index.php/SmoothMouseLook
 */

using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Camera-Control/Smooth Mouse Look")]
public class SmoothMouseLook : MonoBehaviour
{

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

    public float SensitivityX = 15F;
    public float SensitivityY = 15F;

    public float MinimumX = -360F;
    public float MaximumX = 360F;

    public float MinimumY = -60F;
    public float MaximumY = 60F;

    public float FrameCounter = 20;

    private float _rotationX;
    private float _rotationY;

    private readonly List<float> _rotArrayX = new List<float>();
    private readonly List<float> _rotArrayY = new List<float>();
    private float _rotAverageX;
    private float _rotAverageY;

    private Quaternion _originalRotation;
    private Quaternion _parentOriginalRotation;

    //---------------------------------------------------------------------
    // Messages
    //---------------------------------------------------------------------

    protected void Update()
    {
        if (!Input.GetButton("Fire2")) return;

        switch (axes)
        {
            case RotationAxes.MouseXAndY:
                {
                    _rotAverageY = 0f;
                    _rotAverageX = 0f;

                    _rotationY += Input.GetAxis("Mouse Y") * SensitivityY;
                    _rotationY = Mathf.Clamp(_rotationY, MinimumY, MaximumY);

                    _rotationX += Input.GetAxis("Mouse X") * SensitivityX;

                    _rotArrayY.Add(_rotationY);
                    _rotArrayX.Add(_rotationX);

                    if (_rotArrayY.Count >= FrameCounter)
                    {
                        _rotArrayY.RemoveAt(0);
                    }
                    if (_rotArrayX.Count >= FrameCounter)
                    {
                        _rotArrayX.RemoveAt(0);
                    }

                    foreach (var t in _rotArrayY) _rotAverageY += t;
                    foreach (var t in _rotArrayX) _rotAverageX += t;

                    _rotAverageY /= _rotArrayY.Count;
                    _rotAverageX /= _rotArrayX.Count;

                    _rotAverageY = ClampAngle(_rotAverageY, MinimumY, MaximumY);
                    _rotAverageX = ClampAngle(_rotAverageX, MinimumX, MaximumX);

                    var yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
                    var xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);

                    //transform.localRotation = _originalRotation * xQuaternion * yQuaternion;
                    transform.localRotation = _originalRotation * yQuaternion;
                    transform.parent.localRotation = _parentOriginalRotation * xQuaternion;

                    break;
                }
            case RotationAxes.MouseX:
                {
                    _rotAverageX = 0f;

                    _rotationX += Input.GetAxis("Mouse X") * SensitivityX;

                    _rotArrayX.Add(_rotationX);

                    if (_rotArrayX.Count >= FrameCounter)
                    {
                        _rotArrayX.RemoveAt(0);
                    }

                    foreach (var t in _rotArrayX) _rotAverageX += t;

                    _rotAverageX /= _rotArrayX.Count;

                    _rotAverageX = ClampAngle(_rotAverageX, MinimumX, MaximumX);

                    Quaternion xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);
                    //transform.localRotation = _originalRotation * xQuaternion;
                    transform.parent.localRotation = _parentOriginalRotation * xQuaternion;
                    break;
                }
            default:
                {
                    _rotAverageY = 0f;

                    _rotationY += Input.GetAxis("Mouse Y") * SensitivityY;

                    _rotArrayY.Add(_rotationY);

                    if (_rotArrayY.Count >= FrameCounter)
                    {
                        _rotArrayY.RemoveAt(0);
                    }

                    foreach (var t in _rotArrayY) _rotAverageY += t;

                    _rotAverageY /= _rotArrayY.Count;

                    _rotAverageY = ClampAngle(_rotAverageY, MinimumY, MaximumY);

                    Quaternion yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
                    transform.localRotation = _originalRotation * yQuaternion;
                    break;
                }
        }
    }

    void Start()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        _originalRotation = transform.localRotation;
        _parentOriginalRotation = transform.parent.localRotation;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }
}