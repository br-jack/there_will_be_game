using UnityEngine;
using System.Collections;

public class Football : MonoBehaviour
{
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // ToDo: Play a kick sound + add particle effects
        if (collision.gameObject.CompareTag("Hammer") || collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Ball kicked by: " + collision.gameObject.name);
        }
    }

    public IEnumerator ResetBall(float delay)
    {
        yield return new WaitForSeconds(delay);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        Debug.Log("Ball reset to center pitch.");
    }
}