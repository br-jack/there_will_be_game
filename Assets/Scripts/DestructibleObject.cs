using UnityEngine;
using System.Collections;

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
        }

    void Start() {
        myRenderer = GetComponent<MeshRenderer>();
        myCollider = GetComponent<Collider>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;
        if (!collision.gameObject.CompareTag("Player")) return;

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

        GameObject fragments = Instantiate(fragmentsPrefab,
                                           transform.position,
                                           transform.rotation);
        fragments.SetActive(true);

        foreach (Rigidbody rb in fragments.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = false;
            rb.AddExplosionForce(explosionForce, impactPoint, explosionRadius, 1f);
            Destroy(rb.gameObject, 10f);
        }

        StartCoroutine(HandleRespawn());
        myRenderer.enabled = false;
        myCollider.enabled = false;
    }

    IEnumerator HandleRespawn()
    {
        yield return new WaitForSeconds(30f);
        myRenderer.enabled = true;
        myCollider.enabled = true;
        broken = false;
    }
}