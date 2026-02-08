using UnityEngine;

public class BodyHit : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag("Player"))
            return;

        GetComponentInParent<EnemyMovement>().Die();
    }
}
