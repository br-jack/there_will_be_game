using UnityEngine;
public struct EnemyAttack
public class StandardEnemyAI : MonoBehaviour
{
    // References
    public GameObject shield;
    private Rigidbody rb;
    private AudioSource _shieldBreakAudioSource;
    private StandardEnemySpawner spawner;

    [Header("Movement")]
    [SerializeField] private float speed = 5f;

    // The distance the Enemy stops at the player should be less than the player's attack range.
    [SerializeField] private float stopFromPlayerDistance = 1.5f;
    [SerializeField] private float smoothVelocity = 0.35f;
    [SerializeField] private float rotationSpeed = 8f;

    public bool IsKnockedBack { get; private set; }
    public bool IsDying { get; private set; }
    public bool ShieldWasJustHit { get; private set; }
    public event Action OnDied;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        _shieldBreakAudioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsDying)
        {
            KillEnemy();
            return;
        }

        if (IsKnockedBack)
        {
            HandleKnockback();
            return;
        }
    }

    private void HandleKnockback()
    {
        
    }

    private void KillEnemy()
    {
        
    }
}
