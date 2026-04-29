using Hammer;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class FireTask : BaseTask
{
    private bool hammerIgnited = false;
    private bool rewardSpawned = false;
    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private GameObject infiniteFirePowerUpPrefab;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private string rewardMessage = "The eternal flame boon has been granted";
    [SerializeField] private int buildingsRequired = 7;
    [SerializeField] private Transform villageTarget;
    private int buildingsBurned = 0;

    [Header("Arrow Targeting")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private BurnableBuilding[] burnableBuildings;

    [SerializeField] private Transform fireBoonSpawnPoint;

    [SerializeField] private horseMovementGaits horseMovement;
    [SerializeField] private TargetHammer targetHammer;
    [SerializeField] private AttackHitbox hammerHitbox;
    [SerializeField] private float boonSpawnFreezeTime = 4f;

    void Start()
    {
        taskName = "Burn down the village";
        taskDescription = "Ignite the hammer";
        StartTask();
    }

    private void Update()
    {
        if (hammerIgnited && buildingsBurned < buildingsRequired && !isComplete && !hammerFireController.IsOnFire)
        {
            hammerIgnited = false;
            taskDescription = "Ignite the hammer";
            TaskHUD.Instance.RefreshUI();
        }
    }

    public void HammerIgnited()
    {
        if (hammerIgnited || isComplete) return;

        hammerIgnited = true;
        Transform nearestBuilding = GetNearestUnburnedBuilding();
        TaskArrowManager.Instance.PointTo(nearestBuilding);
        taskDescription = $"Burn the village ({buildingsBurned}/{buildingsRequired} buildings burned)";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public void BuildingBurned()
    {
        if (isComplete) return;

        buildingsBurned++;

        if (buildingsBurned > buildingsRequired)
        {
            buildingsBurned = buildingsRequired;
        }

        if (buildingsBurned >= buildingsRequired)
        {
            taskDescription = "All buildings burned! Return to the statue.";
        }
        else
        {
            taskDescription = $"Burn the village ({buildingsBurned}/{buildingsRequired} buildings burned)";
            Transform nearestBuilding = GetNearestUnburnedBuilding();
            TaskArrowManager.Instance.PointTo(nearestBuilding);
        }
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (buildingsBurned >= buildingsRequired)
        { 
            if (!rewardSpawned && powerUpSpawner != null && infiniteFirePowerUpPrefab != null)
            {
                StartCoroutine(SpawnFireBoonSequence());
                rewardSpawned = true;
                TaskArrowManager.Instance.HideArrow();
            }
            CompleteTask();
        }
    }

    private IEnumerator SpawnFireBoonSequence()
    {
        SetPlayerControl(false);

        powerUpSpawner.SpawnSpecificPowerUp(infiniteFirePowerUpPrefab, rewardMessage, fireBoonSpawnPoint);

        yield return new WaitForSeconds(boonSpawnFreezeTime);

        SetPlayerControl(true);
    }

    private Transform GetNearestUnburnedBuilding()
    {
        if (playerTransform == null || burnableBuildings == null || burnableBuildings.Length == 0)
        {
            return null;
        }

        BurnableBuilding nearestBuilding = null;
        float nearestDistanceSqr = float.MaxValue;

        for (int i = 0; i < burnableBuildings.Length; i++)
        {
            BurnableBuilding building = burnableBuildings[i];

            if (building == null)
            {
                continue;
            }

            if (building.IsBurning)
            {
                continue;
            }

            float distanceSqr = (building.transform.position - playerTransform.position).sqrMagnitude;

            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestBuilding = building;
            }
        }

        return nearestBuilding != null ? nearestBuilding.transform : null;
    }

    private void SetPlayerControl(bool enabled)
    {
        horseMovement.canControl = enabled;
        targetHammer.canControl = enabled;
        Collider hitboxCollider = hammerHitbox.GetComponent<Collider>();
        hitboxCollider.enabled = enabled;
    }
}