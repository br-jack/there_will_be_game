using System.Collections;
using TMPro;
using UnityEngine;

public class PowerUpSpawner : MonoBehaviour
{
    public GameObject[] powerUpPrefabs;

    // Defining the spawn area for the power ups
    public float minX = -10.0f;
    public float maxX = 10.0f;
    public float minZ = -10.0f;
    public float maxZ = 10.0f;
    public float spawnHeight = 12.0f;

    public float spawnInterval = 120.0f;
    public bool spawnImmediately = false;

    // Gonna add this here but haven't actually added the text yet
    public TextMeshProUGUI boonText;
    public float messageDuration = 3.0f;

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
            yield return StartCoroutine(ShowBoonMessage());
        }

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            yield return StartCoroutine(ShowBoonMessage());
            SpawnRandomPowerUp();
        }
    }

    private void SpawnRandomPowerUp()
    {

        int randomIndex = Random.Range(0, powerUpPrefabs.Length);

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        Vector3 spawnPosition = new Vector3(randomX, spawnHeight, randomZ);

        Instantiate(powerUpPrefabs[randomIndex], spawnPosition, Quaternion.identity);
    }

    private IEnumerator ShowBoonMessage()
    {
        // Can delete this once i've actually added in the text in the UI
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
