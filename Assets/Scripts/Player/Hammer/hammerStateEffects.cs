using UnityEngine;

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

    public Color _stillAndWalkColor;
    public Color _trotColor;
    public Color _canterColor;
    public Color _gallopColor;
    public Color _ultraGallopColor;

    [SerializeField] private Material defaultHammerHeadMaterial;

    private Material hammerHeadMaterial;

    public void updateGait(gait newGait)
    {
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
        
        
        
        hammerHeadMaterial = new Material(defaultHammerHeadMaterial);
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
                hammerHeadMaterial.color = _stillAndWalkColor;
                break;
            case gait.walking: 
                _chargeLinesMain.startColor = _stillAndWalkColor; 
                _embersMain.startColor = _stillAndWalkColor;
                _glowMain.startColor = _stillAndWalkColor;
                _ghostsMain.startColor = _stillAndWalkColor;
                _trailsMain.startColor = _stillAndWalkColor;
                hammerHeadMaterial.color = _stillAndWalkColor;
                break;
            case gait.trotting: 
                _chargeLinesMain.startColor = _trotColor;
                _embersMain.startColor = _trotColor;
                _glowMain.startColor = _trotColor;
                _ghostsMain.startColor = _trotColor;
                _trailsMain.startColor = _trotColor;
                hammerHeadMaterial.color = _trotColor;
                break;
            case gait.cantering: 
                _chargeLinesMain.startColor = _canterColor;
                _embersMain.startColor = _canterColor;
                _glowMain.startColor = _canterColor;
                _ghostsMain.startColor = _canterColor;
                _trailsMain.startColor = _canterColor;
                hammerHeadMaterial.color = _canterColor;
                break;
            case gait.galloping: 
                _chargeLinesMain.startColor = _gallopColor;
                _embersMain.startColor = _gallopColor;
                _glowMain.startColor = _gallopColor;
                _ghostsMain.startColor = _gallopColor;
                _trailsMain.startColor = _gallopColor;
                hammerHeadMaterial.color = _gallopColor;
                break;
            case gait.ultraGalloping: 
                _chargeLinesMain.startColor =  _ultraGallopColor;
                _embersMain.startColor = _ultraGallopColor;
                _glowMain.startColor = _ultraGallopColor;
                _ghostsMain.startColor = _ultraGallopColor;
                _trailsMain.startColor = _ultraGallopColor;
                hammerHeadMaterial.color = _ultraGallopColor;
                break;
            case gait.vulcan: 
                _chargeLinesMain.startColor = Color.black; 
                _embersMain.startColor = Color.black;
                _glowMain.startColor = Color.black;
                _ghostsMain.startColor = Color.black;
                _trailsMain.startColor = Color.black;
                hammerHeadMaterial.color = Color.black;
                break;
            default: 
                break;
            
        }
        
        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }
}
