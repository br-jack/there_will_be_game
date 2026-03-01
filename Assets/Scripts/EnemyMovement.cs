using System;
using UnityEngine;

public struct ZoneSettings
{
    public float radius;
    public float weight;
}
public struct AdaptiveRepelSettings
{
    public float speedThreshold;
    public ZoneSettings normal;
    public ZoneSettings tooSlow;
    public ZoneSettings GetSettings(float planarSpeed)
    {
        if (planarSpeed < speedThreshold) return tooSlow;
        return normal;
    }
}

public struct StillStuckSettings
{
    public float minSpeedThreshold;
    public float lateralNudgeDisplacement;

}

public class EnemyMovement : MonoBehaviour
{
    /**
    Variables explained:
    defaultSpeed: speed E travels at when OUT of formation
    formationSpeed: speed E travels at when IN formation
    spawner: every E is connected to a spawner
    _formationTarget: when IN formation, the position that E should move towards (in formation grid)
    _attackTarget: when NOT in formation, the position that E should move towards
    */
    public float defaultSpeed;
    public float formationSpeed;
    public EnemySpawner spawner;
    public GameObject shield;
    [Header("Smooth Movement Variables")]
    public float arriveRadius = 0.6f; // Distance from target before enemy slows down.
    public float stopRadius = 0.18f; // Distance from target when enemy halts.
    public float velocityLerp = 0.25f; // How quickly desired velocity becomes current velocity.
    /* 
    There are 2 zones implemented around each player that prevent clumping.
    
    REPELS in a smaller zone
    A force of repulsion of strength {repelStrength} repels 2 enemies inversely proportional to distance between.
    It only has effect when distance is less than the {repelRadius} threshold. (It's analagous to the force-distance graph of the strong nuclear force but for repulsion rather than attraction.)

    SLOWS down in a slightly bigger zone: {slowZoneRadius, slowFactor}
    */
    [Header("Prevent Clumping")]
    public float slotJitterRadius = 0.08f;
    public float minForwardClearLen = 0.45f;

    private Health _playerHealthRef;
    private Transform _playerTransformRef;
    private Rigidbody _rb;

    public bool isKnockedback;
    public bool shieldWasJustHit = false;
    private Vector3 _formationTarget;
    private Vector3 _attackTarget;
    private Vector3 _formationForward = Vector3.forward;
    private Vector3 _slotJitter = Vector3.zero;
    public bool hasFormationTarget;
    private bool _hasAttackTarget;
    private float _sideBias = 1f;

    public AdaptiveRepelSettings repel = new AdaptiveRepelSettings
    {
        speedThreshold = 0.12f,
        normal = new ZoneSettings
        {
            radius = 1.15f, // Distance where enemies repel each other (analagous to protons in a nucleus, prevents clumping together)
            weight = 1.15f // Decides how strong the repel force is 
        },
        tooSlow = new ZoneSettings
        {
            radius = 1.6f,
            weight = 2.2f
        }
    };

    public ZoneSettings slowZone = new ZoneSettings
    {
        radius = 0.9f, // radius of zone (max distance of effect)
        weight = 0.35f // min speed multiplier when heavily crowded
    }; 
    public StillStuckSettings stillStuckSettings = new StillStuckSettings 
    { 
        minSpeedThreshold = 0.18f, // min speed to count as 'stuck'
        lateralNudgeDisplacement = 0.6f // distance that the enemy is moved if it's not moving (stuck)
    };

    private void Awake()
    {
        _sideBias = UnityEngine.Random.value > 0.5f ? 1f : -1f;
        Vector2 jitter2D = UnityEngine.Random.insideUnitCircle * slotJitterRadius;
        _slotJitter = new Vector3(jitter2D.x, 0f, jitter2D.y);
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        GameObject playerRef = GameObject.FindWithTag("Player");

        _playerTransformRef = playerRef.transform;
        _playerHealthRef = playerRef.GetComponent<Health>();

        // Default values (if none set)
        if (defaultSpeed <= 0f) defaultSpeed = 3f;
        if (formationSpeed <= 0f) formationSpeed = (2f/3f) * defaultSpeed;
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private Vector3 GetTargetPosition()
    {
        /**
        IF in formation, go to formation.
        ELSE IF has attack target, go to attack target.
        ELSE go to player.*/
        if (hasFormationTarget) return _formationTarget;
        if (_hasAttackTarget) return _attackTarget;
        return _playerTransformRef.position;
    }

    private bool IsBlockedAhead(Vector3 s, float d, Vector3 targetDirection)
    {
        // Checks for another enemy close to the direction it's moving, if they are not in the direction of movement, it's fine
        if (targetDirection.sqrMagnitude > 0.0001f)
        {
            float proj = Vector3.Dot(-s.normalized, targetDirection);
            if (proj > 0.4f && d < minForwardClearLen)
            {
                return true;
            }
        }
        return false;
    }

    private Vector3 RotateEnemy(Vector3 targetDirection, Vector3 separation, float repelWeight)
    {
        if (separation != Vector3.zero)
        {
            targetDirection = (targetDirection + separation * repelWeight).normalized;
        }
        // Ensure all enemies in the formation face the same direction (usually towards the player)
        Vector3 faceDir;
        if (hasFormationTarget)
        {
            if (_formationForward.sqrMagnitude > 0.0001f)
            {
                faceDir = _formationForward;
            }
            else
            { // This is here to make the control flow more clear.
                faceDir = targetDirection;
            }
        }
        else
        {
            faceDir = targetDirection;
        }
        if (faceDir != Vector3.zero)
        {
            Vector3 flatDirection = new Vector3(faceDir.x, 0, faceDir.z).normalized;
            if (flatDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(flatDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 5f);
            }
        }
        return targetDirection;
    }

    private float ModifySpeed(float distanceToTarget, float proximitySlowFactor, bool blockedAhead)
    {
        // If in formation and not close to player, prefer formation speed
        float speed = defaultSpeed;
        float distanceToFormation = Vector3.Distance(transform.position, _formationTarget);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransformRef.position);
        if (hasFormationTarget && (distanceToPlayer > spawner.breakFormationDistance) && (distanceToFormation < spawner.joinFormationDistance))
        {
            speed = formationSpeed;
        }
        if (hasFormationTarget && distanceToFormation < 0.5f)
        {
            speed = formationSpeed;
        }

        // The speed needs to be adjusted if the player is really close to its desired position.
        // If the enemy is (~ almost exactly) at the target, it shouldn't move
        if (distanceToTarget < stopRadius) speed = 0f;

        // If the enemy is almost at the target, it should slow down slightly to prevent overshooting
        else if (distanceToTarget < arriveRadius)
        { 
            speed *= distanceToTarget / arriveRadius;
        }

        speed *= proximitySlowFactor;
        if (blockedAhead) speed *= 0.5f;
        
        return speed;
    }
    private void FixedUpdate()
    {
        shieldWasJustHit = false;
        // Don't use AI if enemy was just hit.
        if (isKnockedback) return;

        //TODO use a* pathfinding instead

        Vector3 targetPosition = GetTargetPosition();

        // toTarget
        Vector3 toTarget = targetPosition - transform.position;
        toTarget.y = 0f;
        Vector3 targetDirection = toTarget.sqrMagnitude > 0.0001f ? toTarget.normalized : Vector3.zero;
        float distanceToTarget = toTarget.magnitude;
        
        // Separation and congestion-aware steering
        Vector3 separation = Vector3.zero;
        Vector3 currentVel = _rb.linearVelocity;
        float planarSpeed = new Vector3(currentVel.x, 0f, currentVel.z).magnitude;

        ZoneSettings curRepel = repel.GetSettings(planarSpeed);

        // float activeSepRadius = horizSpeed < stuckSpeedThreshold ? stuckSeparationRadius : separationRadius;
        // float activeSepWeight = horizSpeed < stuckSpeedThreshold ? stuckSeparationWeight : separationWeight;
        // float activeSepRadiusSq = activeSepRadius * activeSepRadius;

        float proximitySlowFactor = 1f;
        bool blockedAhead = false;

        if (spawner != null)
        {

            for (int i = 0; i < spawner.aliveEnemies.Count; i++)
            {
                EnemyMovement E = spawner.aliveEnemies[i];
                if (E == null || E == this) continue;

                /*
                WORKING OUT VARIABLES FOR REPEL ZONE AND SLOW ZONE
                s = distance (vector), d = displacement (scalar), d2 = displacement squared
                (from this enemy position -> to other enemy E position)*/
                Vector3 s = transform.position - E.transform.position;
                s.y = 0f;
                float d = s.magnitude;
                float d2 = s.sqrMagnitude;

                if (d < 0.01f || d > curRepel.radius) continue;

                // (direction of vector from enemy to E) * (linearly decreasing function wrt distance between enemy and E)
                separation += s.normalized * (1f - d / curRepel.radius);

                // ONLY IF distance (between this enemy and CLOSEST E) is too small, slow down this enemy (to prevent being stuck)
                if (d < slowZone.radius)
                {
                    float thisProximitySlowFactor = 1f - (1f - slowZone.weight) * (1f - d / slowZone.radius);

                    // ONLY concerns the single E closest to this enemy. 
                    proximitySlowFactor = Mathf.Min(proximitySlowFactor, thisProximitySlowFactor);
                }

                // blockedAhead = IsBlockedAhead(s, d, targetDirection);
                blockedAhead = blockedAhead || IsBlockedAhead(s, d, targetDirection);
            }
        }

        /*
        Sets base speed based on whether the enemy is close enough to the formation or not.
        Then, modifies base speed depending if it's already ~in formation place or very nearby */
        float speed = ModifySpeed(distanceToTarget, proximitySlowFactor, blockedAhead);

        targetDirection = RotateEnemy(targetDirection, separation, curRepel.weight);

        Vector3 movement = targetDirection * speed;

        // If the enemy is moving extremely slowly (stuck), push it perpendicular to the direction of travel
        if (hasFormationTarget && (speed > 0f) && (planarSpeed < stillStuckSettings.minSpeedThreshold))
        {
            Vector3 right = Vector3.Cross(Vector3.up, _formationForward).normalized;
            movement += right * stillStuckSettings.lateralNudgeDisplacement * _sideBias;
        }

        _rb.linearVelocity = Vector3.Lerp(
            currentVel,
            new Vector3(movement.x, currentVel.y, movement.z),
            Mathf.Clamp01(velocityLerp)
        );
    }

    private void OnCollisionEnter(Collision E)
    {
        if (E.gameObject.CompareTag("Arena") && isKnockedback)
        {
            isKnockedback = false;
        }
    }

    public void Die()
    {
        Destroy(gameObject);
    }

    public void ApplyKnockback(Vector3 force)
    {
        isKnockedback = true;

        _rb.linearVelocity = Vector3.zero;
        _rb.AddForce(force, ForceMode.Impulse);
    }

    public void BreakShield()
    {
        if (shield != null)
        {
            Destroy(shield);
            shield = null;
        }
    }

    public bool HasShield()
    {
        return shield != null;
    }

    public void MarkShieldHit()
    {
        shieldWasJustHit = true;
    }

    private void OnDisable()
    {
        if (spawner != null)
        {
            spawner.aliveEnemies.Remove(this);
            spawner.UpdateFormationTargets();
        }
    }

    public void SetFormationTarget(Vector3 target) 
    {
        SetFormationTarget(target, _formationForward);
    }

    public void SetFormationTarget(Vector3 target, Vector3 forward)
    {
        _formationTarget = target + _slotJitter;
        _formationForward = forward.sqrMagnitude > 0.0001f ? forward.normalized : _formationForward;
        hasFormationTarget = true;
    }

    public void ClearFormationTarget()
    {
        _formationTarget = Vector3.zero;
        hasFormationTarget = false;
    }

    public void SetAttackTarget(Vector3 target)
    {
        _attackTarget = target;
        _hasAttackTarget = true;
    }
}
