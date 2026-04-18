using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]

public struct RandomMovementSettings
{
    public float radius;
    public float minInterval;
    public float maxInterval;
    public float speed;
}

public struct RunAwaySettings
{
    public float startRunningRadius;
    public float stopRunningRadius;
    public float speed;
    public float targetDistance;
    public float refreshInterval;

}

public class CivilianAI : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }
}