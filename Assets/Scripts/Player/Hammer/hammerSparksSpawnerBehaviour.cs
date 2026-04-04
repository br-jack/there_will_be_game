using UnityEngine;

public class hammerSparksSpawnerBehaviour : MonoBehaviour
{
    public hammerSpeedState hammerSpeedState;
    public hammerChargeState hammerChargeState;
    private ParticleSystem.MainModule _mmod;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (hammerSpeedState) {
            case hammerSpeedState.still: 
                _mmod.startColor = Color.white;
                break;
            case hammerSpeedState.trotting: 
                _mmod.startColor = Color.blue;
                break;
            case hammerSpeedState.cantering: 
                _mmod.startColor = Color.yellow;
                break;
            case hammerSpeedState.galloping: 
                _mmod.startColor = Color.red;
                break;
            case hammerSpeedState.ultraGalloping: 
                _mmod.startColor = Color.magenta;
                break;
            case hammerSpeedState.vulcan: 
                _mmod.startColor = Color.white;
                break;
            default: 
                break;
        }
    }
}
