using UnityEngine;

public class BrazierProximityIgnition : MonoBehaviour
{
    [SerializeField] private Transform hammerHead;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private FireTask fireTask;
    [SerializeField] private BoxCollider igniteTrigger;

    private void Reset()
    {
        igniteTrigger = GetComponent<BoxCollider>();
        igniteTrigger.isTrigger = true;
    }

    private void Update()
    {
        if (IsPointInsideBoxCollider(igniteTrigger, hammerHead.position))
        {
            bool wasAlreadyOnFire = hammerFireController.IsOnFire;

            hammerFireController.IgniteHammer();

            if (!wasAlreadyOnFire && fireTask != null)
            {
                fireTask.HammerIgnited();
            }
        }
    }

    private bool IsPointInsideBoxCollider(BoxCollider box, Vector3 worldPoint)
    {
        Vector3 localPoint = box.transform.InverseTransformPoint(worldPoint) - box.center;
        Vector3 halfSize = box.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= halfSize.x && Mathf.Abs(localPoint.y) <= halfSize.y && Mathf.Abs(localPoint.z) <= halfSize.z;
    }
}
