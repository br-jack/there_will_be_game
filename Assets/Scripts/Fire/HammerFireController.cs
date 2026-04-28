using UnityEngine;

public class HammerFireController : MonoBehaviour
{
    [SerializeField] private bool isOnFire = false;
    [SerializeField] private float fireDuration = 15f;

    [SerializeField] private bool infiniteFireUnlocked = false;
    [SerializeField] private bool useInfiniteFire = false;

    [Header("Fire VFX")]
    [SerializeField] private GameObject fireVisualParent;
    [SerializeField] private ParticleSystem[] fireParticleSystems;

    private float fireTimer = 0f;

    public bool IsOnFire => isOnFire;
    public bool InfiniteFireUnlocked => infiniteFireUnlocked;
    public bool UseInfiniteFire => useInfiniteFire;

    public bool HasEternalFireBoonActive => isOnFire && useInfiniteFire;

    private void Awake()
    {
        if (fireParticleSystems == null || fireParticleSystems.Length == 0)
        {
            if (fireVisualParent != null)
            {
                fireParticleSystems = fireVisualParent.GetComponentsInChildren<ParticleSystem>(true);
            }
        }
    }

    private void Start()
    {
        StopFireVisuals();
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
            PlayFireVisuals();
        }

        if (!useInfiniteFire)
        {
            fireTimer = fireDuration;
        }
    }

    public void ExtinguishHammer()
    {
        if (!isOnFire)
            return;

        isOnFire = false;
        fireTimer = 0f;

        StopFireVisuals();
    }

    public void UnlockInfiniteFire()
    {
        infiniteFireUnlocked = true;
        useInfiniteFire = true;
    }

    public void SetInfiniteFireActive(bool active)
    {
        if (!infiniteFireUnlocked)
            return;

        useInfiniteFire = active;
    }

    public float GetRemainingFireTime()
    {
        return fireTimer;
    }

    private void PlayFireVisuals()
    {
        if (fireVisualParent != null)
        {
            fireVisualParent.SetActive(true);
        }

        foreach (ParticleSystem particleSystem in fireParticleSystems)
        {
            if (particleSystem != null)
            {
                particleSystem.Play(true);
            }
        }
    }

    private void StopFireVisuals()
    {
        if (fireVisualParent != null)
        {
            fireVisualParent.SetActive(false);
        }

        foreach (ParticleSystem particleSystem in fireParticleSystems)
        {
            if (particleSystem != null)
            {
                particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
