using UnityEngine;

public class hammerStateEffects : MonoBehaviour
{
    public hammerSpeedState hammerSpeedState;
    public hammerChargeState hammerChargeState;

    
    
    
    public ParticleSystem embers;
    private ParticleSystem.EmissionModule _embersEmission;
    private ParticleSystem.MinMaxGradient _embersCOLGradient;

    public ParticleSystem chargeLines;
    private ParticleSystem.EmissionModule _chargeLinesEmission;
    private ParticleSystem.MainModule _chargeLinesMain;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //_chargeLinesMain = chargeLines.main;
        //_chargeLinesEmission = chargeLines.emission;
        //_embersEmission = embers.emission;
        //_embersCOLGradient = embers.colorOverLifetime.color;
    }

    // Update is called once per frame
    void Update()
    {   
        _chargeLinesMain = chargeLines.main;
        _chargeLinesEmission = chargeLines.emission;
        _embersEmission = embers.emission;
        _embersCOLGradient = embers.colorOverLifetime.color;
        /*
        switch (hammerChargeState)
        {
            case hammerChargeState.uncharged:
                _embersEmission.enabled = false;
                _chargeLinesEmission.enabled = false;
                break;
            case hammerChargeState.charging: 
                _embersEmission.enabled = false;
                _chargeLinesEmission.enabled = true; 
                break;
            case hammerChargeState.charged: 
                _embersEmission.enabled = true;
                _chargeLinesEmission.enabled = true; 
                break;
        }
        */
        switch (hammerSpeedState) {
            case hammerSpeedState.still: 
                _chargeLinesMain.startColor = Color.white;
                _embersCOLGradient.colorMin = Color.white;
                break;
            case hammerSpeedState.trotting: 
                _chargeLinesMain.startColor = Color.blue;
                _embersCOLGradient.colorMin = Color.blue;
                break;
            case hammerSpeedState.cantering: 
                _chargeLinesMain.startColor = Color.yellow;
                _embersCOLGradient.colorMin = Color.yellow;
                break;
            case hammerSpeedState.galloping: 
                _chargeLinesMain.startColor = Color.red;
                _embersCOLGradient.colorMin = Color.red;
                break;
            case hammerSpeedState.ultraGalloping: 
                _chargeLinesMain.startColor = Color.magenta;
                _embersCOLGradient.colorMin = Color.magenta;
                break;
            case hammerSpeedState.vulcan: 
                _chargeLinesMain.startColor = Color.white;
                _embersCOLGradient.colorMin = Color.white;
                break;
            default: 
                break;
            
        }
        //something like this may be necessary, not sure how well the pointers work
        //_chargeLinesCOL.color = _embersCOLGradient; 
    }
}
