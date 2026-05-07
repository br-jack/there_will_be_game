using TMPro;
using System.Collections;
using UnityEngine;

public class WaveAnnouncerUI : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private float messageDuration = 3f;

    // subscribing and unsubscribing to the wave event in spawner
    private void OnEnable()
    {
        spawner.OnWaveStarted += ShowWave;
    }

    private void OnDisable()
    {
        spawner.OnWaveStarted -= ShowWave;
    }

    private void ShowWave(int waveNumber)
    {
        StartCoroutine(ShowMessage(waveNumber));
    }

    private IEnumerator ShowMessage(int waveNumber)
    {
        waveText.text = "Wave " + waveNumber;
        waveText.enabled = true;
        yield return new WaitForSeconds(messageDuration);
        waveText.enabled = false;
    }
}
