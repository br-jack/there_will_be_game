using UnityEngine;
using UnityEngine.Events;

public class slamHitbox : MonoBehaviour
{

    private BoxCollider _collider;
    public UnityEvent<Vector3> slam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
