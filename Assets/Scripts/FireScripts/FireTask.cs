using UnityEngine;

public class FireTask : BaseTask
{
    private bool hammerIgnited = false;
    private bool buildingBurned = false;
    private bool rewardSpawned = false;
    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private GameObject infiniteFirePowerUpPrefab;
    [SerializeField] private HammerFireController hammerFireController;
    [SerializeField] private string rewardMessage = "The eternal flame boon has been granted";

    void Start()
    {
        taskName = "Burn the building";
        taskDescription = "Ignite the hammer";
        StartTask();
    }

    private void Update()
    {
        if (hammerIgnited && !buildingBurned && !isComplete)
        {
            if (!hammerFireController.IsOnFire)
            {
                hammerIgnited = false;
                taskDescription = "Ignite the hammer";
                TaskHUD.Instance.RefreshUI();
            }
        }
    }

    public void HammerIgnited()
    {
        if (hammerIgnited || buildingBurned) return;

        hammerIgnited = true;

        taskDescription = "Go to the building";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public void BuildingBurned()
    {
        if (buildingBurned) return;

        buildingBurned = true;

        taskDescription = "Completed!";
        TaskHUD.Instance.RefreshUI();

        CheckCompletion();
    }

    public override void CheckCompletion()
    {
        if (hammerIgnited && buildingBurned)
        { 
            if (!rewardSpawned && powerUpSpawner != null && infiniteFirePowerUpPrefab != null)
            {
                powerUpSpawner.SpawnSpecificPowerUp(infiniteFirePowerUpPrefab, rewardMessage);
                rewardSpawned = true;
            }
            CompleteTask();
        }
    }
}