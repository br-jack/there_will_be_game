using UnityEngine;

public class HammerFireController : MonoBehaviour
{
    [SerializeField] private bool isOnFire = false;
    [SerializeField] private GameObject fireVisual;
    [SerializeField] private ParticleSystem fireParticles;

    public bool IsOnFire => isOnFire;

    private void Start()
    {
        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (fireParticles != null)
            fireParticles.Stop();
    }

    public void IgniteHammer()
    {
        if (isOnFire) return;

        isOnFire = true;

        if (fireVisual != null)
            fireVisual.SetActive(true);

        if (fireParticles != null)
            fireParticles.Play();

        Debug.Log("Hammer is now on fire.");
    }

    public void ExtinguishHammer()
    {
        if (!isOnFire) return;

        isOnFire = false;

        if (fireVisual != null)
            fireVisual.SetActive(false);

        if (fireParticles != null)
            fireParticles.Stop();

        Debug.Log("Hammer fire extinguished.");
    }
}
