using UnityEngine;

public class ArmIK : MonoBehaviour
{
    [SerializeField] private Transform shoulder;
    [SerializeField] private Transform elbow;
    [SerializeField] private Transform wrist;
    [SerializeField] private Transform target;
    [SerializeField] private Transform elbowHint;
    [SerializeField] private float followSpeed = 15f;
    [SerializeField] private bool matchWristRotation = true;

    // Cached bone lengths
    private float _upperArmLength;
    private float _lowerArmLength;


    private void Start()
    {
        if (shoulder == null || elbow == null || wrist == null)
        {
            Debug.LogError($"ArmIK on {gameObject.name}: bone references are missing!");
            enabled = false;
            return;
        }

        _upperArmLength = Vector3.Distance(shoulder.position, elbow.position);
        _lowerArmLength = Vector3.Distance(elbow.position, wrist.position);
    }

    // LateUpdate runs after Unity's own Animator, so this overrides base pose correctly
    private void LateUpdate()
    {
        if (target == null)
            return;

        SolveIK();
    }

    private void SolveIK()
    {
        Vector3 rootPos = shoulder.position;
        Vector3 targetPos = target.position;

        float totalLength = _upperArmLength + _lowerArmLength;

        // Direction and clamped distance from shoulder to target
        Vector3 toTarget = targetPos - rootPos;
        float dist = Mathf.Min(toTarget.magnitude, totalLength * 0.9999f);
        Vector3 toTargetDir = toTarget.normalized;

        // --- Determine the bend plane from the elbow hint ---
        Vector3 bendNormal;
        if (elbowHint != null)
        {
            Vector3 toHint = (elbowHint.position - rootPos).normalized;
            // The plane normal is perpendicular to both the arm direction and hint direction
            bendNormal = Vector3.Cross(toTargetDir, toHint).normalized;
        }
        else
        {
            // Fallback: use the shoulder's current up as the bend plane normal
            bendNormal = Vector3.Cross(toTargetDir, shoulder.up).normalized;
        }

        // Guard against degenerate cross product (hint is collinear with arm direction)
        if (bendNormal.sqrMagnitude < 0.001f)
            bendNormal = Vector3.Cross(toTargetDir, Vector3.up).normalized;

        // The direction the elbow should bend toward
        Vector3 bendDir = Vector3.Cross(bendNormal, toTargetDir).normalized;

        // --- Law of cosines to find angles ---
        float a = _upperArmLength;  // shoulder -> elbow
        float b = _lowerArmLength;  // elbow -> wrist
        float c = dist;             // shoulder -> target

        // Angle at the shoulder (between upper arm and the line to target)
        float cosA = Mathf.Clamp((a * a + c * c - b * b) / (2f * a * c), -1f, 1f);
        float angleA = Mathf.Acos(cosA) * Mathf.Rad2Deg;

        // Angle at the elbow (between upper arm and lower arm)
        float cosC = Mathf.Clamp((a * a + b * b - c * c) / (2f * a * b), -1f, 1f);
        float angleC = Mathf.Acos(cosC) * Mathf.Rad2Deg;

        // --- Compute elbow world position from the solved angles ---
        // Rotate toTargetDir by angleA around bendNormal to get upper arm direction
        Quaternion shoulderBendRot = Quaternion.AngleAxis(angleA, bendNormal);
        Vector3 upperArmDir = shoulderBendRot * toTargetDir;
        Vector3 elbowPos = rootPos + upperArmDir * a;

        // --- Apply rotations to bones ---
        // Shoulder: point from shoulder toward the solved elbow position
        Quaternion targetShoulderRot = Quaternion.FromToRotation(
            shoulder.rotation * GetLocalBoneAxis(shoulder, elbow),
            (elbowPos - shoulder.position).normalized
        ) * shoulder.rotation;

        shoulder.rotation = Quaternion.Slerp(shoulder.rotation, targetShoulderRot, followSpeed * Time.deltaTime);

        // Elbow: point from elbow toward the target (wrist end)
        Quaternion targetElbowRot = Quaternion.FromToRotation(
            elbow.rotation * GetLocalBoneAxis(elbow, wrist),
            (targetPos - elbow.position).normalized
        ) * elbow.rotation;

        elbow.rotation = Quaternion.Slerp(elbow.rotation, targetElbowRot, followSpeed * Time.deltaTime);

        // Wrist: match the target's rotation for a proper grip
        if (matchWristRotation)
        {
            wrist.rotation = Quaternion.Slerp(wrist.rotation, target.rotation, followSpeed * Time.deltaTime);
        }
    }

    private Vector3 GetLocalBoneAxis(Transform parent, Transform child)
    {
        return parent.InverseTransformDirection((child.position - parent.position).normalized);
    }

    private void OnDrawGizmos()
    {
        if (shoulder == null || elbow == null || wrist == null)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(shoulder.position, elbow.position);
        Gizmos.DrawLine(elbow.position, wrist.position);
        Gizmos.DrawWireSphere(elbow.position, 0.03f);

        if (target != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.05f);
            Gizmos.DrawLine(wrist.position, target.position);
        }

        if (elbowHint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(elbowHint.position, 0.04f);
            Gizmos.DrawLine(elbow.position, elbowHint.position);
        }
    }
}
