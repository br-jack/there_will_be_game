using System.Collections;
using TMPro;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{

    public GameObject[] powerUpPrefabs;

    public Transform spawnPoint;
    public float spawnHeightOffset = 10.0f;

    public TextMeshProUGUI boonText;
    public float messageDuration = 3.0f;

    // keeping this here cause i eventually want to add a soundclip for this
    public AudioClip boonFanfare;
    public AudioSource audioSource;

    [SerializeField] private Transform player;
    [SerializeField] private float spawnRadius = 5f;

    public Camera mainCamera;
    public Camera cutsceneCamera;

    public Vector3 cutsceneOffset = new Vector3(0f, 4f, -8f);
    public float cutsceneLookSmooth = 5f;
    public float returnDelay = 1f;
    private Vector3 fixedCutscenePosition;

    private GameObject currentSpawnedPowerUp;
    private bool watchingFall = false;

    private void Start()
    {
        if (boonText != null)
        {
            audioSource  = GetComponent<AudioSource>();
            boonText.gameObject.SetActive(false);
            cutsceneCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
        }
    }

    public GameObject SpawnSpecificPowerUp(GameObject powerUpPrefab, string customMessage)
    {
        return SpawnSpecificPowerUp(powerUpPrefab, customMessage, spawnPoint);
    }

    public GameObject SpawnSpecificPowerUp(GameObject powerUpPrefab, string customMessage, Transform chosenSpawnPoint)
    {
        Vector3 spawnPosition = chosenSpawnPoint.position + Vector3.up * spawnHeightOffset;

        currentSpawnedPowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);

        PowerUpPickup pickup = currentSpawnedPowerUp.GetComponent<PowerUpPickup>();

        if (pickup != null)
        {
            pickup.OnLanded += HandlePowerUpLanded;
        }

        if (audioSource != null)
        {
            audioSource.Play();
        }

        StartCoroutine(ShowBoonMessage(customMessage));
        StartCutscene(chosenSpawnPoint);

        return currentSpawnedPowerUp;
    }

    private void StartCutscene(Transform chosenSpawnPoint)
    {
        if (mainCamera != null)
        {
            mainCamera.gameObject.SetActive(false);
        }

        Vector3 directionFromPlayer = (chosenSpawnPoint.position - player.position).normalized;

        fixedCutscenePosition = chosenSpawnPoint.position + directionFromPlayer * 12f + Vector3.up * 6f; // pull back and raise camera

        cutsceneCamera.fieldOfView = 75f;
        cutsceneCamera.transform.position = fixedCutscenePosition;
        cutsceneCamera.gameObject.SetActive(true);

        watchingFall = true;
    }

    private void EndCutscene()
    {
        watchingFall = false;
        cutsceneCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (!watchingFall || cutsceneCamera == null || currentSpawnedPowerUp == null)
        {
            return;
        }

        cutsceneCamera.transform.position = fixedCutscenePosition;

        Vector3 lookTarget = currentSpawnedPowerUp.transform.position + Vector3.up * 1.5f; // adjust the height as needed
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - cutsceneCamera.transform.position);
        cutsceneCamera.transform.rotation = Quaternion.Slerp(cutsceneCamera.transform.rotation, targetRotation, Time.deltaTime * cutsceneLookSmooth);
    }

    private void HandlePowerUpLanded()
    {
        StartCoroutine(ReturnToMainCameraAfterDelay());
    }

    private IEnumerator ReturnToMainCameraAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        if (currentSpawnedPowerUp != null)
        {
            PowerUpPickup pickup = currentSpawnedPowerUp.GetComponent<PowerUpPickup>();

            if (pickup != null)
            {
                pickup.OnLanded -= HandlePowerUpLanded;
            }
        }

        EndCutscene();
    }

    private IEnumerator ShowBoonMessage(string message)
    {
        if (boonText == null)
        {
            yield break;
        }

        boonText.text = message;
        boonText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        boonText.gameObject.SetActive(false);
    }
}