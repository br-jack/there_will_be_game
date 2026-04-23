using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Hammer;

public class TutorialManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject introOverlay;
    [SerializeField] private Image dimBackground;
    [SerializeField] private TextMeshProUGUI introText;

    [Header("Message")]
    [SerializeField] private string message = "A divine force guides your first steps. \n Learn to move, fight and shape Fear and Awe.";
    [SerializeField] private float letterDelay = 0.07f;
    [SerializeField] private float holdTime = 2f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInTime = 1.5f;
    [SerializeField] private float fadeOutTime = 1.5f;
    [SerializeField] private float maxBackgroundAlpha = 0.75f;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject fearBarUI;
    [SerializeField] private GameObject aweBarUI;
    [SerializeField] private GameObject healthBarUI;
    [SerializeField] private GameObject boonTextUI;
    [SerializeField] private GameObject taskPanelUI;
    [SerializeField] private GameObject waveAnnouncerUI;

    [Header("Tutorial Prompt UI")]
    [SerializeField] private GameObject tutorialPromptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string firstPromptMessage = "Swing your hammer";
    [SerializeField] private string secondPromptMessage = "Jump using the A button";
    [SerializeField] private string thirdPromptMessage = "Complete the task shown on the panel";
    [SerializeField] private int swingsRequired = 3;
    [SerializeField] private int jumpsRequired = 1;

    [Header("Tutorial Task")]
    [SerializeField] private TutorialReachPointTask tutorialTask;

    [Header("Reward")]
    [SerializeField] private PowerUpSpawner powerUpSpawner;
    [SerializeField] private GameObject tutorialRewardPrefab;
    private string rewardMessage = "A boon has been granted";
    private string boonSpawnPromptMessage = "Task complete. A boon has appeared.";
    private string boonCollectPromptMessage = "Collect the boon to receive a blessing";
    private string boonExplanationMessage = "Boons grant powerful blessings that aid your journey.";
    private float boonPromptDelay = 4f;

    [SerializeField] private GameObject tutorialMarker;

    [Header("3D Tutorial Arrow")]
    [SerializeField] private TaskArrow3D taskArrow;

    [Header("Tutorial Exit")]
    [SerializeField] private Transform tutorialDoorTarget;
    [SerializeField] private string exitPromptMessage = "Congratulations, continue to the door to exit the tutorial";
    [SerializeField] private TransitionToMain tutorialDoor;

    [Header("Tutorial Enemy")]
    [SerializeField] private TutorialEnemySpawner tutorialEnemySpawner;
    [SerializeField] private horseMovementGaits horseMovement;
    [SerializeField] private TargetHammer targetHammer;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private float enemyKillUnlockDelay = 1f;
    [SerializeField] private float forcedSlowMoveMultiplier = 0.1f;
    [SerializeField] private AttackHitbox hammerHitbox;

    [Header("Enemy Facing")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float autoFaceTurnSpeed = 5f;
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private string enemyIntroPromptMessage = "An enemy approaches. Watch your health.";
    [SerializeField] private string enemyKillPromptMessage = "Now defeat the enemy to gain Fear and Awe.";

    [Header("Task Panel Intro")]
    private string taskPanelIntroMessage = "Tasks appear on this panel. Complete them to earn rewards.";
    private float taskPanelIntroDelay = 4.5f;
    private float taskPanelPulseDuration = 4f;
    private float taskPanelPulseScale = 1.03f;
    private float taskPanelPulseSpeed = 1f;

    [Header("Arrow Intro")]
    private string arrowIntroMessage = "Sometimes an arrow will guide you to completion";
    private float arrowIntroDelay = 4.5f;
    private float arrowPulseDuration = 4f;
    private float arrowPulseScale = 1.15f;
    private float arrowPulseSpeed = 1f;

    private StandardEnemyAI currentTutorialEnemy;
    private bool enemyPhaseStarted = false;
    private bool enemyHasHitPlayer = false;
    private bool enemyDefeated = false;
    private bool enemyCutsceneFinished = false;


    private GameObject spawnedTutorialReward;
    private bool rewardCollected = false;

    private bool taskStarted = false;
    private bool rewardSpawned = false;

    private bool introFinished = false;
    private bool firstPromptCompleted = false;
    private bool secondPromptCompleted = false;

    private int currentSwings = 0;
    private int currentJumps = 0;

    private bool taskIntroSequenceStarted = false;

    private void OnEnable()
    {
        TargetHammer.OnHammerSwing += HandleHammerSwing;
        horseMovementGaits.OnTutorialJump += HandleJump;
        TaskManager.OnAnyTaskCompleted += HandleTaskCompleted;
        PowerUpPickup.OnPowerUpCollected += HandlePowerUpCollected;
        playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
    }

    private void OnDisable()
    {
        TargetHammer.OnHammerSwing -= HandleHammerSwing;
        horseMovementGaits.OnTutorialJump -= HandleJump;
        TaskManager.OnAnyTaskCompleted -= HandleTaskCompleted;
        PowerUpPickup.OnPowerUpCollected -= HandlePowerUpCollected;
        playerHealth.OnHealthChanged -= HandlePlayerHealthChanged;
    }

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // At the start of the intro hide the gameplay UI and tutorial prompts
        HideGameplayUIAtStart();
        tutorialPromptUI.SetActive(false);

        introOverlay.SetActive(true);

        // Start with invisible background and empty text
        Color background = dimBackground.color;
        background.a = 0f;
        dimBackground.color = background;

        introText.text = "";
        Color textColour = introText.color;
        textColour.a = 1f;
        introText.color = textColour;

        // Fade in dark background
        yield return StartCoroutine(FadeBackground(0f, maxBackgroundAlpha, fadeInTime));

        // Type text letter by letter
        yield return StartCoroutine(TypeText(message));

        // Hold for a moment
        yield return new WaitForSeconds(holdTime);

        // Fade everything back out
        yield return StartCoroutine(FadeOutOverlay());

        introOverlay.SetActive(false);

        tutorialPromptUI.SetActive(true);
        introFinished = true;
        promptText.text = $"{firstPromptMessage} ({currentSwings}/{swingsRequired})";
    }

    private IEnumerator TypeText(string fullMessage)
    {
        fullMessage = fullMessage.Replace("\\n", "\n");
        introText.text = "";

        for (int i = 0; i < fullMessage.Length; i++)
        {
            introText.text += fullMessage[i];
            yield return new WaitForSeconds(letterDelay);
        }
    }

    private IEnumerator FadeBackground(float startAlpha, float endAlpha, float duration)
    {

        float elapsed = 0f;
        Color background = dimBackground.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            background.a = Mathf.Lerp(startAlpha, endAlpha, t);
            dimBackground.color = background;

            yield return null;
        }

        background.a = endAlpha;
        dimBackground.color = background;
    }

    private IEnumerator FadeOutOverlay()
    {
        float elapsed = 0f;

        Color startBackground = dimBackground.color;
        Color startText = introText.color;

        while (elapsed < fadeOutTime)
        {
            elapsed += Time.deltaTime;
            float percent = Mathf.Clamp01(elapsed / fadeOutTime);

            Color background = startBackground;
            background.a = Mathf.Lerp(startBackground.a, 0f, percent);
            dimBackground.color = background;

            Color textColour = startText;
            textColour.a = Mathf.Lerp(startText.a, 0f, percent);
            introText.color = textColour;

            yield return null;
        }

        Color finalBackground = dimBackground.color;
        finalBackground.a = 0f;
        dimBackground.color = finalBackground;

        Color finalTextColour = introText.color;
        finalTextColour.a = 0f;
        introText.color = finalTextColour;
    }

    private void HandleHammerSwing()
    {
        if (introFinished && !firstPromptCompleted)
        {
            currentSwings++;
            currentSwings = Mathf.Min(currentSwings, swingsRequired);
            promptText.text = $"{firstPromptMessage} ({currentSwings}/{swingsRequired})";
            if (currentSwings >= swingsRequired)
            {
                firstPromptCompleted = true;
                currentJumps = 0;
                promptText.text = $"{secondPromptMessage} ({currentJumps}/{jumpsRequired})";
            }
        }
    }

    private void HandleJump()
    {
        if (introFinished && firstPromptCompleted && !secondPromptCompleted)
        {
            currentJumps++;
            currentJumps = Mathf.Min(currentJumps, jumpsRequired);
            promptText.text = $"{secondPromptMessage} ({currentJumps}/{jumpsRequired})";
            if (currentJumps >= jumpsRequired)
            {
                secondPromptCompleted = true;
                if (!taskIntroSequenceStarted)
                {
                    StartCoroutine(ShowTaskPanelIntroSequence());
                }
            }
        }
    }

    private IEnumerator ShowTaskPanelIntroSequence()
    {
        if (taskIntroSequenceStarted)
        {
            yield break;
        }

        taskIntroSequenceStarted = true;
        taskPanelUI.SetActive(true);
        StartCoroutine(PulseTaskPanel());
        promptText.text = taskPanelIntroMessage;
        yield return new WaitForSeconds(taskPanelIntroDelay);
        taskArrow.Show(true);
        StartCoroutine(PulseArrow());
        promptText.text = arrowIntroMessage;
        yield return new WaitForSeconds(arrowIntroDelay);
        promptText.text = thirdPromptMessage;
        StartTutorialTask();
    }

    private IEnumerator PulseArrow()
    {
        if (taskArrow == null)
        {
            yield break;
        }

        Transform arrowTransform = taskArrow.transform;
        Vector3 originalScale = arrowTransform.localScale;
        float elapsed = 0f;

        while (elapsed < arrowPulseDuration)
        {
            elapsed += Time.deltaTime;

            float pulse = 1f + Mathf.Sin(elapsed * arrowPulseSpeed * Mathf.PI * 2f) * (arrowPulseScale - 1f);
            arrowTransform.localScale = originalScale * pulse;

            yield return null;
        }

        arrowTransform.localScale = originalScale;
    }

    private IEnumerator PulseTaskPanel()
    {
        if (taskPanelUI == null)
        {
            yield break;
        }

        RectTransform rectTransform = taskPanelUI.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            yield break;
        }

        Vector3 originalScale = rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < taskPanelPulseDuration)
        {
            elapsed += Time.deltaTime;

            float pulse = 1f + Mathf.Sin(elapsed * taskPanelPulseSpeed * Mathf.PI * 2f) * (taskPanelPulseScale - 1f);
            rectTransform.localScale = originalScale * pulse;

            yield return null;
        }

        rectTransform.localScale = originalScale;
    }

    private void StartTutorialTask()
    {
        if (taskStarted)
        {
            return;
        }

        taskStarted = true;
        tutorialTask.StartTask();
        tutorialMarker.SetActive(true);
        taskArrow.SetTarget(tutorialMarker.transform);
    }

    private void HandleTaskCompleted(BaseTask completedTask)
    {
        if (!taskStarted || rewardSpawned)
        {
            return;
        }

        if (completedTask != tutorialTask)
        {
            return;
        }
        taskArrow.Show(false);

        rewardSpawned = true;
        tutorialMarker.SetActive(false);

        spawnedTutorialReward = powerUpSpawner.SpawnSpecificPowerUp(tutorialRewardPrefab, rewardMessage);

        StartCoroutine(PlayBoonTutorialSequence());
    }

    private IEnumerator PlayBoonTutorialSequence()
    {
        promptText.text = boonSpawnPromptMessage;

        yield return new WaitForSeconds(boonPromptDelay);

        promptText.text = $"{boonCollectPromptMessage}\n\n<size=85%>{boonExplanationMessage}</size>";
    }

    private void HideGameplayUIAtStart()
    {
        taskArrow.Show(false);
        tutorialMarker.SetActive(false);
        fearBarUI.SetActive(false);
        aweBarUI.SetActive(false);
        healthBarUI.SetActive(false);
        boonTextUI.SetActive(false);
        taskPanelUI.SetActive(false);
        waveAnnouncerUI.SetActive(false);
    }

    private void HandlePowerUpCollected(PowerUpPickup collectedPowerUp)
    {
        if (!rewardSpawned || rewardCollected)
        {
            return;
        }

        if (collectedPowerUp.gameObject != spawnedTutorialReward)
        {
            return;
        }

        rewardCollected = true;

        StartTutorialEnemyPhase();
    }

    private void SetPlayerControl(bool enabled)
    {
        horseMovement.canControl = enabled;
        targetHammer.canControl = enabled;
        Collider hitboxCollider = hammerHitbox.GetComponent<Collider>();
        hitboxCollider.enabled = enabled;
    }

    private void StartTutorialEnemyPhase()
    {
        if (enemyPhaseStarted)
        {
            return;
        }

        enemyPhaseStarted = true;
        enemyCutsceneFinished = false;

        // Show health now so the player can see it drop
        healthBarUI.SetActive(true);

        // Lock player while the enemy spawns in
        SetPlayerControl(false);
        SetPlayerForcedSlowMovement(false);

        // Update prompt
        promptText.text = enemyIntroPromptMessage;

        // The door should still remain disabled here
        taskArrow.Show(false);

        currentTutorialEnemy = tutorialEnemySpawner.SpawnTutorialEnemy();
        currentTutorialEnemy.EnableTutorialKillLockMode();
        currentTutorialEnemy.OnDied += HandleTutorialEnemyDied;
        SnapFaceTarget(currentTutorialEnemy.transform);
        SnapCameraToTarget(currentTutorialEnemy.transform);

        StartCoroutine(BeginForcedSlowMovementAfterEnemyCutscene());
    }

    private IEnumerator BeginForcedSlowMovementAfterEnemyCutscene()
    {
        if (tutorialEnemySpawner != null)
        {
            yield return new WaitForSeconds(tutorialEnemySpawner.CutsceneDuration);
        }

        enemyCutsceneFinished = true;

        SetPlayerControl(true);
        SetPlayerForcedSlowMovement(true);

        // Keep the hammer from killing the enemy before the demonstration hit
        if (hammerHitbox != null)
        {
            Collider hitboxCollider = hammerHitbox.GetComponent<Collider>();
            if (hitboxCollider != null)
            {
                hitboxCollider.enabled = false;
            }
        }
    }

    private void HandlePlayerHealthChanged(int current, int max)
    {
        if (!enemyPhaseStarted || enemyHasHitPlayer || enemyDefeated || !enemyCutsceneFinished)
        {
            return;
        }

        if (current < max)
        {
            enemyHasHitPlayer = true;

            // Give controls back now that the player has seen damage
            SetPlayerControl(true);
            SetPlayerForcedSlowMovement(false);

            Collider hitboxCollider = hammerHitbox.GetComponent<Collider>();
            hitboxCollider.enabled = true;

            // Show score + awe so the player can see them update after the kill
            fearBarUI.SetActive(true);
            aweBarUI.SetActive(true);

            // Point the arrow at the enemy now
            taskArrow.SetTarget(currentTutorialEnemy.transform);
            taskArrow.Show(true);

            promptText.text = enemyKillPromptMessage;
            currentTutorialEnemy.SetCanBeKilled(false);
            StartCoroutine(EnableEnemyKillAfterDelay(enemyKillUnlockDelay));
        }
    }

    private IEnumerator EnableEnemyKillAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentTutorialEnemy != null)
        {
            currentTutorialEnemy.SetCanBeKilled(true);
        }
    }

    private void HandleTutorialEnemyDied()
    {
        if (enemyDefeated)
        {
            return;
        }

        enemyDefeated = true;

        currentTutorialEnemy.OnDied -= HandleTutorialEnemyDied;
        tutorialDoor.EnableDoor();
        taskArrow.SetTarget(tutorialDoorTarget);
        taskArrow.Show(true);
        promptText.text = exitPromptMessage;
    }

    private void SnapFaceTarget(Transform target)
    {
        if (playerTransform == null || target == null)
        {
            return;
        }

        Vector3 direction = target.position - playerTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
        playerTransform.rotation = lookRotation * Quaternion.Euler(0f, 45f, 0f);
    }

    private void SnapCameraToTarget(Transform target)
    {
        if (cameraTransform == null || target == null)
        {
            return;
        }

        Vector3 direction = target.position - cameraTransform.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        cameraTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void SetPlayerForcedSlowMovement(bool enabled)
    {
        if (enabled)
        {
            horseMovement.tutorialSpeedMultiplier = forcedSlowMoveMultiplier;
            horseMovement.tutorialAllowJump = false;
        }
        else
        {
            horseMovement.tutorialSpeedMultiplier = 1f;
            horseMovement.tutorialAllowJump = true;
        }
    }
}
