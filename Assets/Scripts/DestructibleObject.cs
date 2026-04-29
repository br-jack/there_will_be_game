using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Score;

public class DestructibleObject : MonoBehaviour
{
    public GameObject fragmentsPrefab;
    public float breakForceThreshold = 15f;
    public float baseExplosionForce = 0.01f;
    public float explosionRadius = 3f;
    private bool broken = false;
    private MeshRenderer myRenderer;
    private Collider myCollider;
    private AudioClip destructionHitSound;
    private AudioClip[] destructionSounds;
    public float soundVolume = 1f;
    public GameObject destructionParticlesPrefab;
    private static Queue<GameObject> activeFragments = new Queue<GameObject>();
    private static int maxFragments = 5000;
    private AudioSource audioSource;
    private ScoreSettings scoreSettings;


    void Awake()
    {
        destructionHitSound = Resources.Load<AudioClip>("DestructionSFX4");
        scoreSettings = Resources.Load<ScoreSettings>("ScoreSettings");


        destructionSounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("DestructionSFX"),
            Resources.Load<AudioClip>("DestructionSFX1"),
            Resources.Load<AudioClip>("DestructionSFX2"),
            Resources.Load<AudioClip>("DestructionSFX3")
        };

        //destructionParticlesPrefab = Resources.Load<GameObject>("BuildingDestructionParticles");

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.75f;
        audioSource.playOnAwake = false;
    }

    public void BreakFromHorseRam(Vector3 impactPoint)
    {
        if (broken)
        {
            return;
        }

        Break(impactPoint);
    }

    void Start() {
        myRenderer = GetComponent<MeshRenderer>();
        myCollider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;
        if (!collision.gameObject.CompareTag("Hammer")) return;

        float impactSpeed = collision.impulse.magnitude / Time.fixedDeltaTime;
        // Debug.Log(impactSpeed);
        if (impactSpeed < breakForceThreshold) return;
        
        // Debug.Log(baseExplosionForce * collision.impulse.magnitude);

        Break(collision.contacts[0].point, baseExplosionForce * collision.impulse.magnitude);
    }

    void Break(Vector3 impactPoint, float explosionForce)
    {
        broken = true;

        AudioClip clip = destructionSounds[Random.Range(0, destructionSounds.Length)];
        if (destructionHitSound != null)
        {
            audioSource.PlayOneShot(destructionHitSound, 0.75f);
        }
        if (clip != null)
        {
            audioSource.PlayOneShot(clip, 0.75f);
        }

        if (destructionParticlesPrefab != null)
            Instantiate(destructionParticlesPrefab, myCollider.bounds.center, Quaternion.identity);

        GameObject fragments = Instantiate(fragmentsPrefab,
                                           transform.position,
                                           transform.rotation);
        fragments.SetActive(true);

        fragments.layer = LayerMask.NameToLayer("Debris");

        foreach (Transform child in fragments.transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null) continue;

            child.gameObject.layer = LayerMask.NameToLayer("Debris");

            while (activeFragments.Count >= maxFragments)
            {
                GameObject oldest = activeFragments.Dequeue();
                if (oldest != null)
                    Destroy(oldest);
            }

            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius, 10f);
            activeFragments.Enqueue(child.gameObject);
            Destroy(child.gameObject, 6f);
        }

        AwardScore();
        SetState(false);

        StartCoroutine(HandleRespawn());
    }

    IEnumerator HandleRespawn()
    {
        yield return new WaitForSeconds(60f);
        SetState(true);
        broken = false;
    }

    void SetState(bool active)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            r.enabled = active;

        foreach (Collider col in GetComponentsInChildren<Collider>(true))
            col.enabled = active;
    }

    private void AwardScore()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return;

        HammerFireController hammerFireController = FindFirstObjectByType<HammerFireController>();

        CharacterController characterController = player.GetComponent<CharacterController>();

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        horseMovementGaits horseMovementGaits = player.GetComponent<horseMovementGaits>();

        if (characterController == null) return;

        List<ScoreComponent> scoreComponents = new List<ScoreComponent>
        {
            // Base score
            new ScoreComponent(scoreSettings.buildingDestructionScore, ScoreType.Building)
        };

        //gait bonus
        if (horseMovementGaits != null)
        {
            switch (horseMovementGaits.getCurrentGait())
            {
                case gait.walking:
                    // no bonus
                    break;
                case gait.trotting:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atATrotBonusScore, ScoreType.atATrot));
                    break;
                case gait.cantering:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atACanterBonusScore, ScoreType.atACanter));
                    break;
                case gait.galloping:
                    scoreComponents.Add(new ScoreComponent(scoreSettings.atAGallopBonusScore, ScoreType.atAGallop));
                    break;
            }
        }

        // Low health bonus
        if (playerHealth != null)
        {
            float healthPercent = (float)playerHealth.Current / playerHealth.Max * 100f;
            if (healthPercent <= scoreSettings.lowHealthThreshold)
            {
                scoreComponents.Add(new ScoreComponent(scoreSettings.lowHealthBonusScore, ScoreType.LowHealth));
            }
        }

        // Air bonus
        if (!characterController.isGrounded)
        {
            scoreComponents.Add(new ScoreComponent(scoreSettings.airBonusScore, ScoreType.Air));
        }

        // On fire bonus
        if (hammerFireController != null && hammerFireController.IsOnFire)
        {
            scoreComponents.Add(new ScoreComponent(scoreSettings.fireBonusScore, ScoreType.OnFire));
        }

        ScoreManager.Instance.AddScore(scoreComponents);
    }

}