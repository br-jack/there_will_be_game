using UnityEngine;

public class hammerStateEffects : MonoBehaviour
{
    public hammerSpeedState hammerSpeedState;
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

    private Color _effectColor;


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
        
        switch (hammerSpeedState) {
            case hammerSpeedState.still: 
                _effectColor = Color.white;
                break;
            case hammerSpeedState.walking: 
                _effectColor = Color.green;
                break;
            case hammerSpeedState.trotting: 
                _effectColor = Color.blue;
                break;
            case hammerSpeedState.cantering: 
                _effectColor = Color.yellow;
                break;
            case hammerSpeedState.galloping: 
                _effectColor = Color.red;
                break;
            case hammerSpeedState.ultraGalloping: 
                _effectColor = Color.magenta;
                break;
            case hammerSpeedState.vulcan: 
                _effectColor = Color.black;
                break;
            default: 
                break;
            
        }
        _chargeLinesMain.startColor = _effectColor;
        _embersMain.startColor = _effectColor;
        _glowMain.startColor = _effectColor;
        _ghostsMain.startColor = _effectColor;
        _trailsMain.startColor = _effectColor;
        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }
}
