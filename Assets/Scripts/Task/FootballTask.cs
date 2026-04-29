using Hammer;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FootballTask : BaseTask
{
    private bool Scored = false;
    private bool rewardSpawned = false;

    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private GameObject rewardPowerUpPrefab;
    [SerializeField] private string rewardMessage = "A divine boon has been granted";
    [SerializeField] private Transform footballBoonSpawnPoint;

    [SerializeField] private horseMovementGaits horseMovement;
    [SerializeField] private TargetHammer targetHammer;
    [SerializeField] private AttackHitbox hammerHitbox;
    [SerializeField] private float boonSpawnFreezeTime = 4f;
    public int goals = 0;

    void Start()
    {
        taskName = "Score a goal against AS Roma FC";
        taskDescription = $"{goals}/1 Scored";
        StartTask();
    }

    public void goalScored()
    {
        Scored = true;
        goals++;
        Debug.Log($"Goals scored: {goals}/1");
        taskDescription = $"{goals}/1 Scored";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    private void Update()
    {
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            CheckCompletion();
        }
    }

    public override void CheckCompletion()
    {
        /*if (!scored)
        {
            return;
        }*/

        if (!rewardSpawned && powerUpSpawner != null && rewardPowerUpPrefab != null)
        {
            StartCoroutine(SpawnFootballBoonSequence());
            rewardSpawned = true;
        }

        GameObject[] walls = GameObject.FindGameObjectsWithTag("PitchWall");

        foreach (GameObject wall in walls)
        {
            wall.SetActive(false);
        }

        CompleteTask();
    }

    private IEnumerator SpawnFootballBoonSequence()
    {
        SetPlayerControl(false);

        powerUpSpawner.SpawnSpecificPowerUp(rewardPowerUpPrefab, rewardMessage, footballBoonSpawnPoint);

        yield return new WaitForSeconds(boonSpawnFreezeTime);

        SetPlayerControl(true);
    }

    private void SetPlayerControl(bool enabled)
    {
        horseMovement.canControl = enabled;
        targetHammer.canControl = enabled;
        Collider hitboxCollider = hammerHitbox.GetComponent<Collider>();
        hitboxCollider.enabled = enabled;
    }
}