using System.Collections;
using UnityEngine;

public class BurnableBuilding : MonoBehaviour
{
    [SerializeField] private GameObject buildingFireVisual;
    [SerializeField] private ParticleSystem buildingFireParticles;
    [SerializeField] private float burnDuration = 5f;
    [SerializeField] private FireTask fireTask;

    private bool isBurning = false;
    public bool IsBurning => isBurning;

    private void Start()
    {
        if (buildingFireVisual != null)
            buildingFireVisual.SetActive(false);

        if (buildingFireParticles != null)
            buildingFireParticles.Stop();
    }

    public void IgniteBuilding()
    {
        if (isBurning) return;

        isBurning = true;

        if (buildingFireVisual != null)
            buildingFireVisual.SetActive(true);

        if (buildingFireParticles != null)
            buildingFireParticles.Play();

        if (fireTask != null)
        {
            fireTask.BuildingBurned();
        }

        Debug.Log("Building has been set on fire");

        StartCoroutine(BurnDown());
    }

    private IEnumerator BurnDown()
    {
        yield return new WaitForSeconds(burnDuration);
        Destroy(gameObject);
    }
}
