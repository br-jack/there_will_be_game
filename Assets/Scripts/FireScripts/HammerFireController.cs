using UnityEngine;

public class HammerFireController : MonoBehaviour
{
    [SerializeField] private bool isOnFire = false;
    [SerializeField] private float fireDuration = 15f;

    [SerializeField] private bool infiniteFireUnlocked = false;
    [SerializeField] private bool useInfiniteFire = false;

    [SerializeField] private GameObject fireVisual;
    [SerializeField] private ParticleSystem fireParticles;

    private float fireTimer = 0f;

    public bool IsOnFire => isOnFire;
    public bool InfiniteFireUnlocked => infiniteFireUnlocked;
    public bool UseInfiniteFire => useInfiniteFire;

    private void Start()
    {
        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (fireParticles != null)
            fireParticles.Stop();
    }

    private void Update()
    {
        if (!isOnFire)
            return;

        if (useInfiniteFire)
            return;

        fireTimer -= Time.deltaTime;

        if (fireTimer <= 0f)
        {
            ExtinguishHammer();
        }
    }

    public void IgniteHammer()
    {
        if (!isOnFire)
        {
            isOnFire = true;

            if (fireVisual != null)
                fireVisual.SetActive(true);

            if (fireParticles != null)
                fireParticles.Play();
        }

        if (!useInfiniteFire)
        {
            fireTimer = fireDuration;
            Debug.Log($"Hammer fire refreshed to {fireDuration} seconds.");
        }
        else
        {
            Debug.Log("Hammer is now on fire infinitely.");
        }
    }

    public void ExtinguishHammer()
    {
        if (!isOnFire) return;

        isOnFire = false;
        fireTimer = 0f;

        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (fireParticles != null)
            fireParticles.Stop();

        Debug.Log("Hammer fire extinguished.");
    }

    public void UnlockInfiniteFire()
    {
        infiniteFireUnlocked = true;
        useInfiniteFire = true;

        Debug.Log("Infinite fire unlocked.");
    }

    public void SetInfiniteFireActive(bool active)
    {
        if (!infiniteFireUnlocked)
            return;

        useInfiniteFire = active;
        Debug.Log("Infinite fire active: " + useInfiniteFire);
    }

    public float GetRemainingFireTime()
    {
        return fireTimer;
    }
}
