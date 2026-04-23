using System.Collections;
using UnityEngine;
using Score;

public class EnemyBurnable : MonoBehaviour
{
    [SerializeField] private StandardEnemyAI enemyAI;

    [Header("Burn Settings")]
    [SerializeField] private float burnDuration = 2.5f;
    [SerializeField] private bool fireballKillAwardsScore = true;
    [SerializeField] private int fireballKillScore = 60;

    [SerializeField] private GameObject burnVisual;
    [SerializeField] private ParticleSystem burnParticles;

    private Coroutine burnRoutine;
    private bool isBurning;
    public bool IsBurning => isBurning;

    private void Awake()
    {
        if (enemyAI == null)
            enemyAI = GetComponent<StandardEnemyAI>();
    }

    public void ApplyBurn(Vector3 fireballSourcePosition)
    {
        if (enemyAI == null) return;
        if (enemyAI.IsDying) return;
        if (enemyAI.IsKnockedBack) return;
        if (!enemyAI.CanBeKilled) return;

        if (burnRoutine != null)
        {
            StopCoroutine(burnRoutine);
        }

        burnRoutine = StartCoroutine(BurnThenKill(fireballSourcePosition));
    }

    private IEnumerator BurnThenKill(Vector3 sourcePosition)
    {
        isBurning = true;

        if (burnVisual != null)
            burnVisual.SetActive(true);

        if (burnParticles != null && !burnParticles.isPlaying)
            burnParticles.Play();

        yield return new WaitForSeconds(burnDuration);

        if (enemyAI != null && !enemyAI.IsDying && enemyAI.CanBeKilled)
        {
            if (fireballKillAwardsScore && ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(new System.Collections.Generic.List<ScoreComponent>
                {
                    new ScoreComponent(fireballKillScore, ScoreType.OnFire)
                });
            }

            enemyAI.KilledByFire(sourcePosition);
        }

        isBurning = false;
        burnRoutine = null;
    }

    public bool IsBurning => isBurning;
}