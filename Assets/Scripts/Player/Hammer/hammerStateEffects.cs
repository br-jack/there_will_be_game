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

    //public Color _effectColor;
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
                _chargeLinesMain.startColor = Color.white; 
                _embersMain.startColor = Color.white;
                _glowMain.startColor = Color.white;
                _ghostsMain.startColor = Color.white;
                _trailsMain.startColor = Color.white;
                break;
            case gait.walking: 
                _chargeLinesMain.startColor = Color.green; 
                _embersMain.startColor = Color.green;
                _glowMain.startColor = Color.green;
                _ghostsMain.startColor = Color.green;
                _trailsMain.startColor = Color.green;
                break;
            case gait.trotting: 
                _chargeLinesMain.startColor = Color.blue; 
                _embersMain.startColor = Color.blue;
                _glowMain.startColor = Color.blue;
                _ghostsMain.startColor = Color.blue;
                _trailsMain.startColor = Color.blue;
                break;
            case gait.cantering: 
                _chargeLinesMain.startColor =  Color.yellow;
                _embersMain.startColor = Color.yellow;
                _glowMain.startColor = Color.yellow;
                _ghostsMain.startColor = Color.yellow;
                _trailsMain.startColor = Color.yellow;
                break;
            case gait.galloping: 
                _chargeLinesMain.startColor = Color.red; 
                _embersMain.startColor = Color.red;
                _glowMain.startColor = Color.red;
                _ghostsMain.startColor = Color.red;
                _trailsMain.startColor = Color.red;
                break;
            case gait.ultraGalloping: 
                _chargeLinesMain.startColor =  Color.magenta;
                _embersMain.startColor = Color.magenta;
                _glowMain.startColor = Color.magenta;
                _ghostsMain.startColor = Color.magenta;
                _trailsMain.startColor = Color.magenta;
                break;
            case gait.vulcan: 
                _chargeLinesMain.startColor = Color.black; 
                _embersMain.startColor = Color.black;
                _glowMain.startColor = Color.black;
                _ghostsMain.startColor = Color.black;
                _trailsMain.startColor = Color.black;
                break;
            default: 
                break;
            
        }
        
        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }
}
