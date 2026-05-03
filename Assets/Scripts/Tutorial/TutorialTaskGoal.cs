using UnityEngine;

public class TutorialTaskGoal : MonoBehaviour
{
    [Header("Task")]
    [SerializeField] private TutorialReachPointTask tutorialTask;

    [Header("Valid Area")]
    [SerializeField] private float validRadius;

    [Header("Horse Foot Points")]
    [SerializeField] private Transform frontLeftFootPoint;
    [SerializeField] private Transform frontRightFootPoint;
    [SerializeField] private Transform backLeftFootPoint;
    [SerializeField] private Transform backRightFootPoint;

    private void Update()
    {
        if (AreAllFeetInsideValidArea())
        {
            tutorialTask.MarkGoalReached();
        }
    }

    private bool AreAllFeetInsideValidArea()
    {
        Vector3 centre = transform.position;
        float allowedRadius = validRadius;
        float allowedRadiusSqr = allowedRadius * allowedRadius;

        return IsPointInsideCircle(frontLeftFootPoint.position, centre, allowedRadiusSqr) &&
               IsPointInsideCircle(frontRightFootPoint.position, centre, allowedRadiusSqr) &&
               IsPointInsideCircle(backLeftFootPoint.position, centre, allowedRadiusSqr) &&
               IsPointInsideCircle(backRightFootPoint.position, centre, allowedRadiusSqr);
    }

    private bool IsPointInsideCircle(Vector3 point, Vector3 centre, float allowedRadiusSqr)
    {
        Vector2 offset = new Vector2(point.x - centre.x, point.z - centre.z);
        return offset.sqrMagnitude <= allowedRadiusSqr;
    }
}
