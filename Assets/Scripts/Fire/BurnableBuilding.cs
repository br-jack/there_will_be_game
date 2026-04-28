using System.Collections;
using UnityEngine;

public class BurnableBuilding : MonoBehaviour
{
    [SerializeField] private GameObject buildingFireVisual;
    [SerializeField] private ParticleSystem buildingFireParticles;
    [SerializeField] private float burnDuration = 5f;
    [SerializeField] private FireTask fireTask;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioClip burningClip;

    private bool isBurning = false;
    public bool IsBurning => isBurning;

    private void Start()
    {
        if (buildingFireVisual != null)
            buildingFireVisual.SetActive(false);

        if (buildingFireParticles != null)
            buildingFireParticles.Stop();

        if (loopSource != null)
        {
            loopSource.clip = burningClip;
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }
    }

    public void IgniteBuilding()
    {
        if (isBurning) return;

        isBurning = true;

        if (buildingFireVisual != null)
            buildingFireVisual.SetActive(true);

        if (buildingFireParticles != null)
            buildingFireParticles.Play();

        if (loopSource != null && burningClip != null)
        {
            loopSource.Play();
        }

        if (fireTask != null)
        {
            fireTask.BuildingBurned();
        }

        StartCoroutine(BurnDown());
    }

    private IEnumerator BurnDown()
    {
        yield return new WaitForSeconds(burnDuration);

        if (loopSource != null)
        {
            loopSource.Stop();
        }
        
        //Destroy(gameObject);
    }
}
