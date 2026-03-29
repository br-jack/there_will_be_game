using UnityEngine;

public class BrazierProximityIgnite : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private float igniteDistance = 10f;

    private bool hasIgnited = false;

    private void Update()
    {
        if (player == null || hammerFireController == null)
            return;

        if (hasIgnited)
            return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= igniteDistance)
        {
            hammerFireController.IgniteHammer();
            hasIgnited = true;
        }
    }
}
