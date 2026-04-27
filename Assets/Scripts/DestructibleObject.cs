using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DestructibleObject : MonoBehaviour
{
    public GameObject fragmentsPrefab;
    public float breakForceThreshold = 15f;
    public float explosionForce = 300f;
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

    void Awake()
    {
        destructionHitSound = Resources.Load<AudioClip>("DestructionSFX4");

        destructionSounds = new AudioClip[]
        {
            Resources.Load<AudioClip>("DestructionSFX"),
            Resources.Load<AudioClip>("DestructionSFX1"),
            Resources.Load<AudioClip>("DestructionSFX2"),
            Resources.Load<AudioClip>("DestructionSFX3")
        };

        //destructionParticlesPrefab = Resources.Load<GameObject>("BuildingDestructionParticles");
    }

    void Start() {
        myRenderer = GetComponent<MeshRenderer>();
        myCollider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;
        if (!collision.gameObject.CompareTag("Hammer")) return;

        float impactSpeed = collision.relativeVelocity.magnitude;
        if (impactSpeed < breakForceThreshold) return;

        Break(collision.contacts[0].point);
    }

    void Break(Vector3 impactPoint)
    {
        broken = true;

        AudioClip clip = destructionSounds[Random.Range(0, destructionSounds.Length)];
        if (clip != null)
            AudioSource.PlayClipAtPoint(destructionHitSound, transform.position, soundVolume);
            AudioSource.PlayClipAtPoint(clip, transform.position, soundVolume);

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
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius, 1f);
            activeFragments.Enqueue(child.gameObject);
            Destroy(child.gameObject, 6f);
        }

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
}