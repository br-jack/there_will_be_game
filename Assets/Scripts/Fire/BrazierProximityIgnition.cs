using UnityEngine;

public class BrazierProximityIgnite : MonoBehaviour
{
    [SerializeField] private Collider hammerHitbox;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private float igniteDistance = 1f;

    [SerializeField] private FireTask fireTask;

    private void Update()
    {
        if (hammerHitbox == null || hammerFireController == null)
            return;

        if (hammerFireController.IsOnFire)
            return;
        Vector3 closestPoint = hammerHitbox.ClosestPoint(transform.position);
        float distance = Vector3.Distance(closestPoint, transform.position);

        if (distance <= igniteDistance)
        {
            hammerFireController.IgniteHammer();
            fireTask.HammerIgnited();
        }
    }
}
