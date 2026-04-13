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

    [SerializeField] private Transform player;
    [SerializeField] private float spawnRadius = 5f;  

    private void Start()
    {
        if (boonText != null)
        {
            boonText.gameObject.SetActive(false);
        }
    }

    public void SpawnSpecificPowerUp(GameObject powerUpPrefab, string customMessage)
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;

        Vector3 spawnPosition = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        spawnPosition += Vector3.up * spawnHeightOffset;

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