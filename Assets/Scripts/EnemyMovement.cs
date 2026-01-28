using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public GameObject playerRef;

    public float speed;

    private Rigidbody _rb;
    
    private void Awake()
    {
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    private void FixedUpdate()
    {
        Vector3 playerPosition = playerRef.transform.position;
        
        Vector3 distanceFromPlayer = playerPosition - transform.position;
        
        _rb.linearVelocity = distanceFromPlayer.normalized * speed;
    }
}
