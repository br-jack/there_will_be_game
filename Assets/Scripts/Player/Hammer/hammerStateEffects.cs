using UnityEngine;
using UnityEngine.Assertions;

public class hammerStateEffects : MonoBehaviour
{
    public gait gait;
    public hammerChargeState hammerChargeState;

    public ParticleSystem embers;
    private ParticleSystem.MainModule _embersMain;

    public ParticleSystem chargeLines;
    private ParticleSystem.MainModule _chargeLinesMain;

    public ParticleSystem glow;
    private ParticleSystem.MainModule _glowMain;

    public ParticleSystem trails;
    private ParticleSystem.MainModule _trailsMain;

    public ParticleSystem ghosts;
    private ParticleSystem.MainModule _ghostsMain;
    public ParticleSystem flash;
    private ParticleSystem.MainModule _flashMain;
    public ParticleSystem explosion;

    public Color _stillAndWalkColor;
    public Color _trotColor;
    public Color _canterColor;
    public Color _gallopColor;
    public Color _ultraGallopColor;

    [SerializeField] private Renderer hammerMesh;
    [SerializeField] private int headMaterialIndex = 2;
    private Material[] _hammerMaterials;

    public void updateGait(gait newGait)
    {
        flash.Play();
        gait = newGait;
    }

    public void updateChargeState(hammerChargeState newHCS)
    {
        hammerChargeState = newHCS;
    }

    public void doSlamEffects()
    {
        explosion.Play();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _embersMain = embers.main;
        _chargeLinesMain = chargeLines.main;
        _glowMain = glow.main;
        _ghostsMain = ghosts.main;
        _trailsMain = trails.main;
        _flashMain = flash.main;

        _hammerMaterials = hammerMesh.materials;

        if (_hammerMaterials[headMaterialIndex].name != "hammer_metal (Instance)")
        {
            Debug.LogWarning("ohno it is not called hammer_metal (Instance), instead is is called: "
                + _hammerMaterials[headMaterialIndex].name);
        }

        _hammerMaterials[headMaterialIndex] = new Material(_hammerMaterials[headMaterialIndex]);
    }

    // Update is called once per frame
    void Update()
    {
        switch (hammerChargeState)
        {

            case hammerChargeState.uncharged:
                embers.Stop();
                chargeLines.Stop();
                glow.Stop();
                break;
            case hammerChargeState.charging:
                embers.Play();
                chargeLines.Play();
                glow.Stop();
                break;
            case hammerChargeState.charged:
                embers.Play();
                chargeLines.Stop();
                glow.Play();
                break;
        }

        switch (gait)
        {
            case gait.still:
                _chargeLinesMain.startColor = _stillAndWalkColor;
                _embersMain.startColor = _stillAndWalkColor;
                _glowMain.startColor = _stillAndWalkColor;
                _ghostsMain.startColor = _stillAndWalkColor;
                _trailsMain.startColor = _stillAndWalkColor;
                _flashMain.startColor = _stillAndWalkColor;
                _hammerMaterials[headMaterialIndex].color = _stillAndWalkColor;
                break;
            case gait.walking:
                _chargeLinesMain.startColor = _stillAndWalkColor;
                _embersMain.startColor = _stillAndWalkColor;
                _glowMain.startColor = _stillAndWalkColor;
                _ghostsMain.startColor = _stillAndWalkColor;
                _trailsMain.startColor = _stillAndWalkColor;
                _flashMain.startColor = _stillAndWalkColor;
                _hammerMaterials[headMaterialIndex].color = _stillAndWalkColor;
                break;
            case gait.trotting:
                _chargeLinesMain.startColor = _trotColor;
                _embersMain.startColor = _trotColor;
                _glowMain.startColor = _trotColor;
                _ghostsMain.startColor = _trotColor;
                _trailsMain.startColor = _trotColor;
                _flashMain.startColor = _trotColor;
                _hammerMaterials[headMaterialIndex].color = _trotColor;
                break;
            case gait.cantering:
                _chargeLinesMain.startColor = _canterColor;
                _embersMain.startColor = _canterColor;
                _glowMain.startColor = _canterColor;
                _ghostsMain.startColor = _canterColor;
                _trailsMain.startColor = _canterColor;
                _flashMain.startColor = _canterColor;
                _hammerMaterials[headMaterialIndex].color = _canterColor;
                break;
            case gait.galloping:
                _chargeLinesMain.startColor = _gallopColor;
                _embersMain.startColor = _gallopColor;
                _glowMain.startColor = _gallopColor;
                _ghostsMain.startColor = _gallopColor;
                _trailsMain.startColor = _gallopColor;
                _flashMain.startColor = _gallopColor;
                _hammerMaterials[headMaterialIndex].color = _gallopColor;
                break;
            case gait.ultraGalloping:
                _chargeLinesMain.startColor = _ultraGallopColor;
                _embersMain.startColor = _ultraGallopColor;
                _glowMain.startColor = _ultraGallopColor;
                _ghostsMain.startColor = _ultraGallopColor;
                _trailsMain.startColor = _ultraGallopColor;
                _flashMain.startColor = _ultraGallopColor;
                _hammerMaterials[headMaterialIndex].color = _ultraGallopColor;
                break;
            case gait.vulcan:
                _chargeLinesMain.startColor = Color.black;
                _embersMain.startColor = Color.black;
                _glowMain.startColor = Color.black;
                _ghostsMain.startColor = Color.black;
                _trailsMain.startColor = Color.black;
                _flashMain.startColor = Color.black;
                _hammerMaterials[headMaterialIndex].color = Color.black;
                break;
            default:
                break;

        }

        //NOTE: this may not be performant
        //Set the renderer's materials this way rather than changing the material
        // because the latter would change the actual asset
        hammerMesh.materials = _hammerMaterials;

        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }

    void OnDestroy()
    {
        for (int i = 0; i < _hammerMaterials.Length; ++i)
        {
            Destroy(_hammerMaterials[i]);
        }
    }
}
