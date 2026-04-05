using UnityEngine;

public class hammerStateEffects : MonoBehaviour
{
    public hammerSpeedState hammerSpeedState;
    public hammerChargeState hammerChargeState;

    
    
    
    public ParticleSystem embers;
    private ParticleSystem.MainModule _embersMain;

    public ParticleSystem chargeLines;
    private ParticleSystem.MainModule _chargeLinesMain;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _embersMain = embers.main;
        _chargeLinesMain = chargeLines.main;
    }

    // Update is called once per frame
    void Update()
    {   

        switch (hammerChargeState)
        {
            
            case hammerChargeState.uncharged:
                embers.Stop();
                chargeLines.Stop();
                break;
            case hammerChargeState.charging: 
                embers.Stop();
                chargeLines.Play();
                break;
            case hammerChargeState.charged: 
                embers.Play();
                chargeLines.Play();
                break;
        }
        
        switch (hammerSpeedState) {
            case hammerSpeedState.still: 
                _chargeLinesMain.startColor = Color.white;
                _embersMain.startColor = Color.white;
                break;
            case hammerSpeedState.trotting: 
                _chargeLinesMain.startColor = Color.blue;
                _embersMain.startColor = Color.blue;
                break;
            case hammerSpeedState.cantering: 
                _chargeLinesMain.startColor = Color.yellow;
                _embersMain.startColor = Color.yellow;
                break;
            case hammerSpeedState.galloping: 
                _chargeLinesMain.startColor = Color.red;
                _embersMain.startColor = Color.red;
                break;
            case hammerSpeedState.ultraGalloping: 
                _chargeLinesMain.startColor = Color.magenta;
                _embersMain.startColor = Color.magenta;
                break;
            case hammerSpeedState.vulcan: 
                _chargeLinesMain.startColor = Color.white;
                _embersMain.startColor = Color.white;
                break;
            default: 
                break;
            
        }
        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }
}
