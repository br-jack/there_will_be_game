using UnityEngine;
using System.Collections.Generic;
using Score;

public class CivilianHit : MonoBehaviour
{
    public hitSound hitSounds;

    private ScoreSettings scoreSettings;
    [SerializeField] private float scoreMultiplier = 0.8f;

    void Awake()
    {
        scoreSettings = Resources.Load<ScoreSettings>("ScoreSettings");
    }

    void OnTriggerEnter(Collider other)
    {
        AttackHitbox attack = other.GetComponent<AttackHitbox>();
        if (attack == null) return;

        CivilianAI civilian = GetComponentInParent<CivilianAI>();
        if (civilian == null) return;

        hitSounds = GameObject.Find("KillSound").GetComponent<hitSound>();
        hitSounds.PlaySFX();

        AwardScore();
        Destroy(civilian.gameObject);
    }

    private int Scaled(int value) => Mathf.RoundToInt(value * scoreMultiplier);

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
            new ScoreComponent(Scaled(scoreSettings.baseScore), ScoreType.Base)
        };

        if (horseMovementGaits != null)
        {
            switch (horseMovementGaits.getCurrentGait())
            {
                case gait.walking:
                    break;
                case gait.trotting:
                    scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.atATrotBonusScore), ScoreType.atATrot));
                    break;
                case gait.cantering:
                    scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.atACanterBonusScore), ScoreType.atACanter));
                    break;
                case gait.galloping:
                    scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.atAGallopBonusScore), ScoreType.atAGallop));
                    break;
            }
        }

        if (playerHealth != null)
        {
            float healthPercent = (float)playerHealth.Current / playerHealth.Max * 100f;
            if (healthPercent <= scoreSettings.lowHealthThreshold)
            {
                scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.lowHealthBonusScore), ScoreType.LowHealth));
            }
        }

        if (!characterController.isGrounded)
        {
            scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.airBonusScore), ScoreType.Air));
        }

        if (hammerFireController != null && hammerFireController.IsOnFire)
        {
            scoreComponents.Add(new ScoreComponent(Scaled(scoreSettings.fireBonusScore), ScoreType.OnFire));
        }

        ScoreManager.Instance.AddScore(scoreComponents);
    }
}
