using UnityEngine;
using System.Collections;

public class DestructibleObjectWOF : MonoBehaviour
{
    private bool broken = false;
    public float breakForceThreshold = 5f;
    public float explosionForce = 300f;
    public float explosionRadius = 3f;
    public float respawnTime = 60f;
    public float debrisCleanupTime = 10f;
    private MeshRenderer myRenderer;
    private Collider myCollider;

    void Start() {
        myRenderer = GetComponent<MeshRenderer>();
        myCollider = GetComponent<Collider>();
    }

    public void Break(Vector3 impactPoint) {
        GetComponent<Fracture>().fractureOptions.asynchronous = false;

        GetComponent<Fracture>().CauseFracture();

        StartCoroutine(HandleRespawn());
    }

    IEnumerator HandleRespawn() {
        myRenderer.enabled = false;
        myCollider.enabled = false;

        GameObject fragments = GameObject.Find(gameObject.name + "Fragments");
        
        yield return new WaitForSeconds(debrisCleanupTime);
        if(fragments != null) Destroy(fragments);

        yield return new WaitForSeconds(respawnTime - debrisCleanupTime);
        myRenderer.enabled = true;
        myCollider.enabled = true;
        broken = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (broken) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            Fracture fractureScript = GetComponent<Fracture>();

            if (fractureScript != null)
            {
                broken = true;
                Break(collision.contacts[0].point);
            }
            else
            {
                Debug.LogWarning("No 'Fracture' script found on " + gameObject.name);
            }
        }
    }
}