using System.Collections;
using System.Collections.Generic;
using Enemy;
using TMPro;
using UnityEngine;

public class TutorialEnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject civilianPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] civilianSpawnPoints;
    


    // Just here in case Jack wants to add SFX
    [Header("Audio")]
    [SerializeField] private AudioClip spawnSound;
    [SerializeField] private AudioSource audioSource;

    [Header("Cutscene Cameras")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera cutsceneCamera;
    [SerializeField] private Transform player;
    [SerializeField] private float returnDelay = 1f;
    [SerializeField] private float cutsceneLookSmooth = 5f;

    private GameObject currentSpawnedEnemy;
    private bool watchingSpawn = false;
    private Vector3 fixedCutscenePosition;

    public float CutsceneDuration => returnDelay;

    private void Start()
    {
        cutsceneCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }

    public StandardEnemyAI SpawnTutorialEnemy()
    {
        currentSpawnedEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        StartCutscene();

        return currentSpawnedEnemy.GetComponent<StandardEnemyAI>();
    }

    public void SpawnTutorialCivilians()
    {
        Debug.Log("spawning civilians!");
        List<CivilianAI> civilians = new List<CivilianAI>();
        foreach (Transform t in civilianSpawnPoints)
        {
            GameObject civilianObj = Instantiate(civilianPrefab,t);
        }
        //return civilians;
    }

    private void StartCutscene()
    {
        mainCamera.gameObject.SetActive(false);

        Vector3 directionFromPlayer = (spawnPoint.position - player.position).normalized;
        fixedCutscenePosition = spawnPoint.position + directionFromPlayer * 10f + Vector3.up * 5f;

        cutsceneCamera.transform.position = fixedCutscenePosition;
        cutsceneCamera.transform.LookAt(spawnPoint.position + Vector3.up * 1.5f);
        cutsceneCamera.gameObject.SetActive(true);
        watchingSpawn = true;

        StartCoroutine(ReturnToMainCameraAfterDelay());
    }

    private IEnumerator ReturnToMainCameraAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);

        watchingSpawn = false;

        cutsceneCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        if (!watchingSpawn)
        {
            return;
        }

        cutsceneCamera.transform.position = fixedCutscenePosition;

        Vector3 lookTarget = currentSpawnedEnemy.transform.position + Vector3.up * 1.5f;
        Quaternion targetRotation = Quaternion.LookRotation(lookTarget - cutsceneCamera.transform.position);
        cutsceneCamera.transform.rotation = Quaternion.Slerp(
            cutsceneCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * cutsceneLookSmooth
        );
    }
}
