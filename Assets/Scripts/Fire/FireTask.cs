using UnityEngine;
using UnityEngine.InputSystem;

public class FireTask : BaseTask
{
    private bool hammerIgnited = false;
    private bool rewardSpawned = false;
    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private GameObject infiniteFirePowerUpPrefab;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private string rewardMessage = "The eternal flame boon has been granted";
    [SerializeField] private int buildingsRequired = 8;
    [SerializeField] private Transform villageTarget;
    private int buildingsBurned = 0;

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
        TaskArrowManager.Instance.PointTo(villageTarget);
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
                powerUpSpawner.SpawnSpecificPowerUp(infiniteFirePowerUpPrefab, rewardMessage);
                rewardSpawned = true;
                TaskArrowManager.Instance.HideArrow();
            }
            CompleteTask();
        }
    }
}