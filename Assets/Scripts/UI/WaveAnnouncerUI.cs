using TMPro;
using System.Collections;
using UnityEngine;

public class WaveAnnouncerUI : MonoBehaviour
{
    [SerializeField] private EnemySpawner spawner;
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private float messageDuration = 3f;

    private void OnEnable()
    {
        if (spawner != null)
        {
            spawner.OnWaveStarted += ShowWave;
        }
    }

    private void OnDisable()
    {
        if (spawner != null)
        {
            spawner.OnWaveStarted -= ShowWave;
        }
    }

    private void ShowWave(int waveNumber)
    {
        StartCoroutine(ShowMessage(waveNumber));
    }

    private IEnumerator ShowMessage(int waveNumber)
    {
        waveText.text = "Wave " + waveNumber;
        waveText.gameObject.SetActive(true);
        yield return new WaitForSeconds(messageDuration);
        waveText.gameObject.SetActive(false);
    }
}
