using UnityEngine;

public class hammerHead : MonoBehaviour
{
    public float forwardSpeed;
    private Transform _tf;
    private Vector3 posPrevFrame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _tf = GetComponent<Transform>();
        posPrevFrame = _tf.position;
    }

    // Update is called once per frame
    void Update()
    {
        //find scalar speed of hammer head in the forwards direction
        Vector3 positionChange = _tf.position - posPrevFrame;
        Vector3 velocityGlobal = positionChange/Time.deltaTime;
        Vector3 velocityLocal =  transform.InverseTransformDirection(velocityGlobal);
        forwardSpeed = velocityLocal.z;
    }
}
