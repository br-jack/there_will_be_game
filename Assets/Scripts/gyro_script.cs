using UnityEngine;

public class gyro_script : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Input.gyro.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.gyro.attitude);
        transform.rotation = Input.gyro.attitude;
    }
}
