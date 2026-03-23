using System.Collections;
using TMPro;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{

    public GameObject[] powerUpPrefabs;

    public Transform[] spawnPoints;
    public float spawnHeightOffset = 10.0f;

    public float spawnInterval = 5.0f;
    public bool spawnImmediately = false;

    public TextMeshProUGUI boonText;
    public float messageDuration = 3.0f;

    // keeping this here cause i eventually want to add a soundclip for this
    public AudioClip boonFanfare;
    public AudioSource audioSource;

    private void Start()
    {
        if (boonText != null)
        {
            boonText.gameObject.SetActive(false);
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        if (spawnImmediately)
        {
            SpawnRandomPowerUp();

            if (audioSource != null && boonFanfare != null)
            {
                audioSource.PlayOneShot(boonFanfare);
            }

            yield return StartCoroutine(ShowBoonMessage());
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            SpawnRandomPowerUp();

            if (audioSource != null && boonFanfare != null)
            {
                audioSource.PlayOneShot(boonFanfare);
            }

            yield return StartCoroutine(ShowBoonMessage());
        }
    }

    private void SpawnRandomPowerUp()
    {

        int randomPowerUpIndex = Random.Range(0, powerUpPrefabs.Length);
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length);

        Transform chosenSpawnPoint = spawnPoints[randomSpawnPointIndex];

        Vector3 spawnPosition = chosenSpawnPoint.position + Vector3.up * spawnHeightOffset;

        Instantiate(powerUpPrefabs[randomPowerUpIndex], spawnPosition, Quaternion.identity);
    }

    private IEnumerator ShowBoonMessage()
    {
        if (boonText == null)
        {
            yield break;
        }

        boonText.text = "A boon has been granted";
        boonText.gameObject.SetActive(true);

        yield return new WaitForSeconds(messageDuration);

        boonText.gameObject.SetActive(false);
    }
}