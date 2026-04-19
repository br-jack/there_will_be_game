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

    /* just used for debugging the valid area and foot points

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        DrawFlatCircle(transform.position, validRadius - extraPadding);

        Gizmos.color = Color.yellow;
        if (frontLeftFootPoint != null) Gizmos.DrawSphere(frontLeftFootPoint.position, 0.05f);
        if (frontRightFootPoint != null) Gizmos.DrawSphere(frontRightFootPoint.position, 0.05f);
        if (backLeftFootPoint != null) Gizmos.DrawSphere(backLeftFootPoint.position, 0.05f);
        if (backRightFootPoint != null) Gizmos.DrawSphere(backRightFootPoint.position, 0.05f);
    }

    private void DrawFlatCircle(Vector3 centre, float radius)
    {
        int segments = 40;
        float angleStep = 360f / segments;

        Vector3 prevPoint = centre + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i * Mathf.Deg2Rad;

            Vector3 nextPoint = centre + new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius
            );

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }
    }*/
}
