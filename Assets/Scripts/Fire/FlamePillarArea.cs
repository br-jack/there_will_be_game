using System.Collections.Generic;
using UnityEngine;

public class FlamePillarArea : MonoBehaviour
{
    [SerializeField] private float lifetime = 2.5f;

    private readonly HashSet<EnemyBurnable> burnedEnemies = new HashSet<EnemyBurnable>();

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyBurnable burnable = other.GetComponentInParent<EnemyBurnable>();
        if (burnable == null)
            return;

        if (burnedEnemies.Contains(burnable))
            return;

        burnedEnemies.Add(burnable);
        burnable.ApplyBurn(transform.position);
    }
}
