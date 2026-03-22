using UnityEngine;
public struct EnemyAttack
public class StandardEnemyAI : MonoBehaviour
{
    public GameObject shield;
    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    // The distance the Enemy stops at the player should be less than the player's attack range.
    [SerializeField] private float stopFromPlayerDistance = 1.5f;
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;
    

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
