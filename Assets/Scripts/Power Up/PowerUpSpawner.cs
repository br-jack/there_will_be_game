using System.Collections;
using TMPro;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{

    public GameObject[] powerUpPrefabs;

    public Transform[] spawnPoints;
    public float spawnHeightOffset = 10.0f;

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
    }

    public void SpawnSpecificPowerUp(GameObject powerUpPrefab, string customMessage)
    {
        int randomSpawnPointIndex = Random.Range(0, spawnPoints.Length);
        Transform chosenSpawnPoint = spawnPoints[randomSpawnPointIndex];

        Vector3 spawnPosition = chosenSpawnPoint.position + Vector3.up * spawnHeightOffset;

        Instantiate(powerUpPrefab, spawnPosition, Quaternion.identity);

        if (audioSource != null && boonFanfare != null)
        {
            audioSource.PlayOneShot(boonFanfare);
        }
        StartCoroutine(ShowBoonMessage(customMessage));
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