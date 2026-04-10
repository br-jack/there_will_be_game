using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class hammerStateEffects : MonoBehaviour
{
    public float materialLerpPerSecond; //How much the material linearly interpolates towards the target colour each second
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

    public Color _stillAndWalkColor;
    public Color _trotColor;
    public Color _canterColor;
    public Color _gallopColor;
    public Color _ultraGallopColor;

    public Material _stillAndWalkMaterial;
    public Material _trotMaterial;
    public Material _canterMaterial;
    public Material _gallopMaterial;
    public Material _ultraGallopMaterial;
    public Material currentHammerMaterial;


    //private Material targetMaterial;
    

    [SerializeField] private MeshRenderer hammerMesh;
    [SerializeField] private int headMaterialIndex = 2;
    private Material[] _hammerMaterials;

    public void updateGait(gait newGait)
    {
        flash.Play(); //flash has a v short delay, so particles should be of the new colour
        gait = newGait;
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

        //Assert.IsTrue(_hammerMaterials[headMaterialIndex].name == "hammer_metal");
        
        //_hammerMaterials[headMaterialIndex] = new Material(_hammerMaterials[headMaterialIndex]);
        //targetMaterial = new Material(_hammerMaterials[headMaterialIndex]);
    }

    // Update is called once per frame
    void Update()
    {   

        switch (hammerChargeState)
        {
            
            case hammerChargeState.uncharged:
                embers.Play();
                chargeLines.Stop();
                glow.Stop();
                break;
            case hammerChargeState.charging: 
                embers.Stop();
                chargeLines.Play();
                glow.Stop();
                break;
            case hammerChargeState.charged: 
                embers.Play();
                chargeLines.Play();
                glow.Play();
                break;
        }
        
        switch (gait) {
            case gait.still: 
                _chargeLinesMain.startColor = _stillAndWalkColor; 
                _embersMain.startColor = _stillAndWalkColor;
                _glowMain.startColor = _stillAndWalkColor;
                _ghostsMain.startColor = _stillAndWalkColor;
                _trailsMain.startColor = _stillAndWalkColor;
                _flashMain.startColor = _stillAndWalkColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_stillAndWalkMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.walking: 
                _chargeLinesMain.startColor = _stillAndWalkColor; 
                _embersMain.startColor = _stillAndWalkColor;
                _glowMain.startColor = _stillAndWalkColor;
                _ghostsMain.startColor = _stillAndWalkColor;
                _trailsMain.startColor = _stillAndWalkColor;
                _flashMain.startColor = _stillAndWalkColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_stillAndWalkMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.trotting: 
                _chargeLinesMain.startColor = _trotColor;
                _embersMain.startColor = _trotColor;
                _glowMain.startColor = _trotColor;
                _ghostsMain.startColor = _trotColor;
                _trailsMain.startColor = _trotColor;
                _flashMain.startColor = _trotColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_trotMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.cantering: 
                _chargeLinesMain.startColor = _canterColor;
                _embersMain.startColor = _canterColor;
                _glowMain.startColor = _canterColor;
                _ghostsMain.startColor = _canterColor;
                _trailsMain.startColor = _canterColor;
                _flashMain.startColor = _canterColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_canterMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.galloping: 
                _chargeLinesMain.startColor = _gallopColor;
                _embersMain.startColor = _gallopColor;
                _glowMain.startColor = _gallopColor;
                _ghostsMain.startColor = _gallopColor;
                _trailsMain.startColor = _gallopColor;
                _flashMain.startColor = _gallopColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_gallopMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.ultraGalloping: 
                _chargeLinesMain.startColor =  _ultraGallopColor;
                _embersMain.startColor = _ultraGallopColor;
                _glowMain.startColor = _ultraGallopColor;
                _ghostsMain.startColor = _ultraGallopColor;
                _trailsMain.startColor = _ultraGallopColor;
                _flashMain.startColor = _ultraGallopColor;
                currentHammerMaterial.Lerp(currentHammerMaterial,_ultraGallopMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            case gait.vulcan: 
                _chargeLinesMain.startColor = Color.black; 
                _embersMain.startColor = Color.black;
                _glowMain.startColor = Color.black;
                _ghostsMain.startColor = Color.black;
                _trailsMain.startColor = Color.black;
                _flashMain.startColor = Color.black;
                //currentHammerMaterial.Lerp(currentHammerMaterial,_stillAndWalkMaterial,materialLerpPerSecond * Time.deltaTime);
                break;
            default: 
                break;
            
        }

        //NOTE: this may not be performant
        //Set the renderer's materials this way rather than changing the material
        // because the latter would change the actual asset
        //_hammerMaterials[headMaterialIndex] = currentHammerMaterial;
        hammerMesh.materials[2] = currentHammerMaterial;

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
