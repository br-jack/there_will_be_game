using System.Drawing.Text;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

public class Gait
{
    public bool accessible;
    public string name;
    public bool midair;
    [SerializeField] private static float _minSpeed;
    [SerializeField] private static float _maxSpeed;
    [SerializeField] private static float _acceleration;
    [SerializeField] private static float _brakeDeceleration; 
    [SerializeField] private static float _staticDeceleration; 
    [SerializeField] private static float _jumpHeight; 
    [SerializeField] private static float _turnSpeed; 
    [SerializeField] private static float _upShiftSpeedMaintenanceBuffer;
    [SerializeField] private static float _upShiftTimeRequisite;
    

    private float _speed;
    private static float _midSpeed;
    private float _timeRemainingToUpShift;
    
    private static Gait _gaitAbove;
    private static Gait _gaitBelow;


    //contructor is probably pointless
    public Gait(
        string new_name, 
        float new_minSpeed, 
        float new_maxSpeed, 
        float new_acceleration,
        float new_brakeDeceleration,
        float new_staticDeceleration,
        float new_jumpHeight,
        float new_upShiftSpeedMaintenanceBuffer,
        float new_upShiftTimeRequisite,
        bool new_accessible,
        Gait gaitBelow = null,
        Gait gaitAbove = null)
    {
        
        accessible = new_accessible;
        name = new_name;
        _minSpeed = new_minSpeed;
        _maxSpeed = new_maxSpeed;
        _midSpeed = (_maxSpeed-_minSpeed)/2.0f;
        _acceleration = new_acceleration;
        _staticDeceleration = new_staticDeceleration;
        _brakeDeceleration = new_brakeDeceleration;
        _jumpHeight = new_jumpHeight;
        _upShiftSpeedMaintenanceBuffer = new_upShiftSpeedMaintenanceBuffer;
        _upShiftTimeRequisite = new_upShiftTimeRequisite;
        _gaitAbove = gaitAbove;
        _gaitBelow = gaitBelow;
    }

    public Gait getNextGait(float throttle, float brake)
    {   
        if (midair) return this; //don't change speed midair!
        
        //otherwise apply throttle + brake
        _speed = _speed + (throttle * _acceleration) - (brake * _brakeDeceleration) - _staticDeceleration;

        //UPSHIFTING
        if ((_speed >= (_maxSpeed - _upShiftSpeedMaintenanceBuffer)) && throttle > 0.0f)
        {
            _timeRemainingToUpShift -= throttle * Time.deltaTime; //time to upshift is based on throttle
            if (_timeRemainingToUpShift <= 0.0f) 
            {   
                if (!_gaitAbove.accessible) 
                {
                    Debug.Log("max speed reached!"); 
                    return this;
                } 
                else 
                {
                    reset();
                    return _gaitAbove; //jump to next gait - could have a method for being shifted into
                }
                
            } 
            else 
            {
                if (brake <= 0.0f) _speed = _maxSpeed; //if not braking, set speed to max to avoid drifting away from upshift speed
                return this;
            }

        } 

        //DOWNSHIFTING: 
        if (_speed < _minSpeed)
        {
            reset();
            return _gaitBelow;
        }

        //MAINTAINING GAIT
        else return this;
        
    }

    private void reset()
    {
        _speed = _midSpeed;
        _timeRemainingToUpShift = _upShiftTimeRequisite;
        midair = false;
    }

    public float getSpeed()
    {
        return _speed;
    }

    public Quaternion getTurnRotation(float turnInput)
    {
        return Quaternion.Euler(0f, turnInput * _turnSpeed * Time.deltaTime, 0f);
    }

    public float getJumpAmount()
    {
        if (midair) {return 0.0f;}
        else {return _jumpHeight;}
    }
}