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
            boonText.gameObject.SetActive(false);
            cutsceneCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
        }
    }

    public void SpawnSpecificPowerUp(GameObject powerUpPrefab, string customMessage)
    {
        Vector3 spawnPosition = spawnPoint.position + Vector3.up * spawnHeightOffset;

        currentSpawnedPowerUp = Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);

        PowerUpPickup pickup = currentSpawnedPowerUp.GetComponent<PowerUpPickup>();
        if (pickup != null)
        {
            pickup.OnLanded += HandlePowerUpLanded;
        }

        if (audioSource != null && boonFanfare != null)
        {
            audioSource.PlayOneShot(boonFanfare);
        }

        StartCoroutine(ShowBoonMessage(customMessage));
        StartCutscene();
    }

    private void StartCutscene()
    {
        mainCamera.gameObject.SetActive(false);
        Vector3 directionFromPlayer = (spawnPoint.position - player.position).normalized;
        fixedCutscenePosition = spawnPoint.position + directionFromPlayer * 12f + Vector3.up * 6f; // pull back and raise camera
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

        Vector3 lookTarget = currentSpawnedPowerUp.transform.position;
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
        PowerUpPickup pickup = currentSpawnedPowerUp.GetComponent<PowerUpPickup>();
        pickup.OnLanded -= HandlePowerUpLanded;

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