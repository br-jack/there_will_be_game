using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class ReactiveRider : MonoBehaviour
{
    [Header("Rider References")]
    [SerializeField] GameObject rider = null;
    [SerializeField] Transform riderRightHand = null;
    //[SerializeField] Transform riderLeftHand = null;

    [Header("Base Rider Position Settings")]
    [SerializeField] bool updatePositionEveryFrame = false;
    [Range(-2F, 2f)]
    [SerializeField] float riderHeightAdjustment = 0;
    [Range(-1.5F, 1.5F)]
    [SerializeField] float riderSaddlePosition = 0;
    [Range(-1F, 1F)]
    [SerializeField] float riderXPositionAdjustment = 0;
    
    [Range(0.0F, 0.5F)]
    [SerializeField] float riderDefaultForwardLean = 0.1f;
    [Range(0.0F, 1F)]
    [SerializeField] float riderForwardLeanMultiplier = 1f;

    [Header("Head LookAt Settings")]
    [SerializeField] bool useHeadIK = true;
    [Range(-2F, 2F)]
    [SerializeField] float defaultHeadTilt = 0f;

    [Header("Leg and Foot Position Settings")]
    [Range(0.0F, 1F)]
    [SerializeField] float riderLegSpread = 0.4f;
    [Range(-1F, 1F)]
    [SerializeField] float riderFootSpread = 0f;
    [Range(-1F, 1F)]
    [SerializeField] float riderFootHeight = 0f;
    [Range(-1F, 1F)]
    [SerializeField] float riderFootPosition = 0f;
    [Range(-45F, 45F)]
    [SerializeField] float riderFootAngle = 0f;
    [Range(-45F, 45F)]
    [SerializeField] float riderFootTilt = 0f;

    [Header("Hand Position Settings")]
    [SerializeField] bool useHandIK = true;
    bool allowHandIKDisable = false;
    [Range(-1F, 1F)]
    [SerializeField] float riderHandSpread = 0f;
    [Range(-1F, 1F)]
    [SerializeField] float riderHandHeight = 0f;
    [Range(-1F, 1F)]
    [SerializeField] float riderHandPosition = 0f;
    Vector3 handRotationCorrection = Vector3.zero;
    Vector3 standardHandRotationCorrection = new Vector3(36.64f, 20, -90);

    [Header("External Targeting")]
    [Range(0, 30)]
    [SerializeField] float chestTwistSpeed = 10;
    [Range(0,180)]
    [SerializeField] float maxChestTwist = 180;
    [Range(0,180)]
    [SerializeField] float maxChestTwistWithHandIKEnabled = 80;

    [Header("Horse References")]
    [SerializeField] GameObject horse = null;
    [SerializeField] GameObject saddleBindPointsPrefab = null;
    [SerializeField] GameObject staticBindPointsPrefab = null;

    //Bindpoints
    Transform rightLegIKTarget = null;
    Transform leftLegIKTarget = null;
    Transform combinedHandIKTarget = null;
    Transform rightHandIKTarget = null;
    Transform leftHandIKTarget = null;
    Transform humanHeadIKTarget = null;
    Transform staticHumanHeadIKTarget = null;
    Transform humanParent = null;
    Transform saddleBindPoint = null;
    Transform actualHorseHead = null;
    Transform staticHorseHead = null;

    //Hand IK
    bool HandIKPassEnabled = true;
    float handIKWeight = 1;
    //Forward Lean
    bool chestForwardLeanEnabled = true;
    float LastForwardLean = 0;
    float calcForwardLean = 0;

    //External Targeting
    Transform exteneralTarget = null;
    float actualChestTwist = 180;
    float externalTargetYOffset = 0;

    //IK Values
    Vector3 rightLegIKPosStart = Vector3.zero;
    Vector3 leftLegIKPosStart = Vector3.zero;
    Quaternion rightLegIKRotStart = Quaternion.identity;
    Quaternion leftLegIKRotStart = Quaternion.identity;
    Vector3 rightHandIKPosStart = Vector3.zero;
    Vector3 leftHandIKPosStart = Vector3.zero;
    Quaternion rightHandIKRotStart = Quaternion.identity;
    Quaternion leftHandIKRotStart = Quaternion.identity;
    Vector3 startCombinedHandPosition = Vector3.zero;
    float distanceCalcAtStartPosition = 0;

    Animator animator = null;
    Quaternion startRot = Quaternion.identity;
    bool forwardLeanTransitionRunning = false; 

    public void SetHandIKPassEnabled(bool isEnabled, float time)
    {
        if (!HasReactiveRiderPrereqs()) return;
        if (isEnabled == false)
        {
            StartCoroutine(DisableHandIKOverTime(time));
        }
        else
        {
            StartCoroutine(EnableHandIKOverTime(time));
        }
    }
    public void SetChestForwardLeanEnabled(bool isEnabled, float time)
    {
        if (!HasReactiveRiderPrereqs()) return;
        if (isEnabled == false)
        {
            StartCoroutine(DisableChestForwardLeanOverTime(time));
        }
        else
        {
            StartCoroutine(EnableChestForwardLeanOverTime(time));
        }
    }
    public void SetExternalTarget(Transform transform, float yOffset)
    {
        if (!HasReactiveRiderPrereqs()) return;
        exteneralTarget = transform;
        externalTargetYOffset = yOffset;
    }


    void Start()
    {
        if (!HasReactiveRiderPrereqs()) return;
        startRot = transform.parent.rotation;
        transform.parent.rotation = Quaternion.identity;
        EnableIKRelayOnCharacter();
        PopulateSaddleBind();
        InstansiateIKTargets();
        PopulateBindPoints();
        PopulateRiderAnimator();
        SetRiderParent();
        PopulateStartCombinedHandPosition();
        SetRiderForwardLean();
        SetRiderLegSpread();
        SetIKStartPositions();
        SetIKStartRotations();
        SetFootPosition();
        SetHandPosition();
        StartCoroutine(PopulateDistanceCalcStartPosition());
    }
    bool HasReactiveRiderPrereqs()
    {
        bool returnValue = true;
        if (horse == null)
        {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: horse variable: has not been assigned!");
            returnValue = false;
        }
        if (staticBindPointsPrefab == null) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: staticBindPointsPrefab: has not been assigned!");
            returnValue = false;
        }
        if (saddleBindPointsPrefab == null) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: saddleBindPointsPrefab: has not been assigned!");
            returnValue = false;
        }
        if (rider == null) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: rider: has not been assigned!");
            returnValue = false;
        }
        if (riderRightHand == null) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: riderRightHand: has not been assigned!");
            returnValue = false;
        }
        return returnValue;
    }
    void EnableIKRelayOnCharacter()
    {
        if (rider.GetComponent<OnAnimatorIKRelay>() == null)
        rider.AddComponent<OnAnimatorIKRelay>();
        rider.GetComponent<OnAnimatorIKRelay>().Saddle = this;
    }
    void CheckHandIKEnabled()
    {
        allowHandIKDisable = false;
        if (!useHandIK)
        {
            SetHandIKPassEnabled(useHandIK,0.1f);
        }
    }
    void PopulateSaddleBind()
    {
        Transform[] saddleChildren = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform transform in saddleChildren)
        {
            if (transform.name == "CharacterBindPoint")
            {
                saddleBindPoint = transform;
                return;
            }
        }
    }
    void InstansiateIKTargets()
    {
        GameObject insStaticBindPoints = Instantiate(staticBindPointsPrefab, transform.parent.position, transform.parent.rotation);
        insStaticBindPoints.transform.SetParent(horse.transform);

        GameObject insSaddleBindPoint = Instantiate(saddleBindPointsPrefab, transform.parent.position, transform.parent.rotation);
        insSaddleBindPoint.transform.SetParent(saddleBindPoint.transform);
    }
    void PopulateBindPoints()
    {
        Transform[] saddleChildren = saddleBindPoint.GetComponentsInChildren<Transform>();
        foreach(Transform transform in saddleChildren)
        {
            if (transform.name == "RR_RightFootIK")
            {
                rightLegIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_LeftFootIK")
            {
                leftLegIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_CombinedHandIK")
            {
                combinedHandIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_RightHandIK")
            {
                rightHandIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_LeftHandIK")
            {
                leftHandIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_SaddleHeadTarget")
            {
                humanHeadIKTarget = transform;
                continue;
            }
            if (transform.name == "RR_HumanParent")
            {
                humanParent = transform;
                continue;
            }
            if (transform.name == "RR_StaticHorseHead")
            {
                staticHorseHead = transform;
                continue;
            }
        }
        Transform[] horseChildren = horse.GetComponentsInChildren<Transform>();
        foreach(Transform transform in horseChildren)
        {
            if (transform.name == "RR_StaticHumanHeadTarget")
            {
                staticHumanHeadIKTarget = transform;
                continue;
            }
            if (transform.name == "Head")
            {
                actualHorseHead = transform;
                continue;
            }
        }
    }
    void PopulateRiderAnimator()
    {
        animator = rider.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: animator: could not find animator on rider!");
        }
    }
    void SetRiderParent()
    {
        rider.transform.parent = humanParent;
        rider.transform.localPosition = Vector3.zero;
        rider.transform.Translate(new Vector3(riderXPositionAdjustment,riderHeightAdjustment+1.6f,riderSaddlePosition));
    }
    IEnumerator PopulateDistanceCalcStartPosition()
    {
        animator.SetFloat("ForwardLean", riderDefaultForwardLean);
        yield return new WaitForSeconds(1f);
        distanceCalcAtStartPosition = Vector3.Distance(riderRightHand.position, actualHorseHead.position);
        allowHandIKDisable = true;
        transform.parent.rotation = startRot;
    }
    void PopulateStartCombinedHandPosition()
    {
        startCombinedHandPosition = combinedHandIKTarget.localPosition;
    }
    void SetRiderForwardLean()
    {
        animator.SetFloat("HipsPosition", riderDefaultForwardLean);
    }
    void SetRiderLegSpread()
    {
        animator.SetFloat("LegSpread",riderLegSpread);
    }
    void SetIKStartPositions()
    {
        rightLegIKPosStart = rightLegIKTarget.localPosition;
        leftLegIKPosStart = leftLegIKTarget.localPosition;
        rightHandIKPosStart = rightHandIKTarget.localPosition;
        leftHandIKPosStart = leftHandIKTarget.localPosition;
    }
    void SetIKStartRotations()
    {
        rightLegIKPosStart = rightLegIKTarget.localPosition;
        leftLegIKPosStart = leftLegIKTarget.localPosition;
        rightHandIKPosStart = rightHandIKTarget.localPosition;
        leftHandIKPosStart = leftHandIKTarget.localPosition;
    }
    void SetFootPosition()
    {
        rightLegIKTarget.localRotation = Quaternion.identity;
        leftLegIKTarget.localRotation = Quaternion.identity;

        rightLegIKTarget.localPosition = rightLegIKPosStart;
        rightLegIKTarget.Translate(riderFootSpread,riderFootHeight,riderFootPosition,Space.Self);
        leftLegIKTarget.localPosition = leftLegIKPosStart;
        leftLegIKTarget.Translate(-riderFootSpread, riderFootHeight, riderFootPosition, Space.Self);

        rightLegIKTarget.localRotation = rightLegIKRotStart;
        rightLegIKTarget.Rotate(new Vector3(riderFootTilt, riderFootAngle,0));
        leftLegIKTarget.localRotation = leftLegIKRotStart;
        leftLegIKTarget.Rotate(new Vector3(riderFootTilt, -riderFootAngle, 0));
    }
    void SetHandPosition()
    {
        rightHandIKTarget.localRotation = Quaternion.identity;
        leftHandIKTarget.localRotation = Quaternion.identity;

        rightHandIKTarget.localPosition = rightHandIKPosStart;
        rightHandIKTarget.Translate(riderHandSpread, riderHandHeight, riderHandPosition, Space.Self);
        leftHandIKTarget.localPosition = leftHandIKPosStart;
        leftHandIKTarget.Translate(-riderHandSpread, riderHandHeight, riderHandPosition, Space.Self);

        if (updatePositionEveryFrame)
        {
            SetHandRotation(rightHandIKTarget, true);
            SetHandRotation(leftHandIKTarget, false);
        }
    }

    public void OnRelayedAnimatorIK()
    {
        if (!HasReactiveRiderPrereqs()) return;
        if (!HasIKPrereqs()) return;
        if (allowHandIKDisable)
        {
            CheckHandIKEnabled();
        }
        SetRiderIK();
    } //OnAnimatorIK

    bool HasIKPrereqs()
    {
        bool returnValue = true;
        if (animator == null) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: animator: could not find animator on rider!");
            returnValue = false;
        }
        if (!RiderHasIKTargets()) {
            Debug.LogError(gameObject.transform.parent.name + ": Reactive Rider: bool HasIKPrereqs(): Internal Error.");
            returnValue = false;
        }
        return returnValue;
    }

    void SetRiderIK()
    {
        SetRiderIKPositionWeights();
        SetRiderIKRotationWeights();
        SetRiderIKPositionGoals();
        SetRiderIKRotationGoals();
        if (useHeadIK)
        {
            SetRiderHeadIKWeight();
            if (exteneralTarget == null)
            {
                SetRiderHeadIKGoals(GetHeadLookAtPosition());
            }
            else {
                SetRiderHeadIKGoals(GetExternalTargetPosition());
            }
        }
    }
    void SetRiderIKPositionWeights()
    {
        animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
        if (HandIKPassEnabled)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, handIKWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, handIKWeight);
        }
    }
    void SetRiderIKRotationWeights()
    {
        animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
        animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
        if (HandIKPassEnabled)
        {
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, handIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, handIKWeight);
        }
    }
    void SetRiderIKPositionGoals(){
        animator.SetIKPosition(AvatarIKGoal.RightFoot, rightLegIKTarget.position);
        animator.SetIKPosition(AvatarIKGoal.LeftFoot, leftLegIKTarget.position);
        if (HandIKPassEnabled)
        {
            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandIKTarget.position);
            animator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandIKTarget.position);
        }
    }
    void SetRiderIKRotationGoals()
    {
        animator.SetIKRotation(AvatarIKGoal.RightFoot, rightLegIKTarget.rotation);
        animator.SetIKRotation(AvatarIKGoal.LeftFoot, leftLegIKTarget.rotation);
        if (HandIKPassEnabled)
        {
            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandIKTarget.rotation);
            animator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandIKTarget.rotation);
        }
    }
    void SetRiderHeadIKWeight()
    {
        animator.SetLookAtWeight(1);
    }
    void SetRiderHeadIKGoals(Vector3 target)
    {
        animator.SetLookAtPosition(target);
    }

    void LateUpdate()
    {
        if (!HasReactiveRiderPrereqs()) return;
        if (updatePositionEveryFrame)
        {
            AdjustRiderPosition();
        }
        else
        {
            SetHandRotation(rightHandIKTarget, true);
            SetHandRotation(leftHandIKTarget, false);
        }
        SetCombinedHandLocation();
        SetRuntimeRiderForwardLean();
        UpdateChestTwistValues();
        SetChestTwist();
    }
    void SetHandRotation(Transform hand, bool isRightHand)
    {
        hand.LookAt(actualHorseHead);
        if(isRightHand)
        {
            hand.Rotate(standardHandRotationCorrection);
        }
        else
        {
            Vector3 mirroredHandVector = standardHandRotationCorrection;
            mirroredHandVector.y = mirroredHandVector.y * -1;
            mirroredHandVector.z = mirroredHandVector.z * -1;
            hand.Rotate(mirroredHandVector);
        }
    }
    void SetCombinedHandLocation()
    {
        Vector3 newPos = staticHorseHead.position;
        if (actualHorseHead.position.y < staticHorseHead.position.y)
        {
            newPos.y = (actualHorseHead.position.y + staticHorseHead.position.y) / 2;
        }
        newPos.z = (actualHorseHead.position.z + staticHorseHead.position.z) / 2;
        newPos.x = (actualHorseHead.position.x + staticHorseHead.position.x) / 2;
        combinedHandIKTarget.position = newPos;
        if (combinedHandIKTarget.transform.localPosition.z > startCombinedHandPosition.z)
        {
            Vector3 locPos = combinedHandIKTarget.localPosition;
            locPos.z = startCombinedHandPosition.z;
            combinedHandIKTarget.localPosition = locPos;
        }
    }
    void SetRuntimeRiderForwardLean()
    {
        if (distanceCalcAtStartPosition == 0) return;
        float positionDistance = Vector3.Distance(riderRightHand.position, actualHorseHead.position);
        calcForwardLean = Mathf.Clamp(((positionDistance - distanceCalcAtStartPosition) * riderForwardLeanMultiplier) + riderDefaultForwardLean, 0, 0.5f);
        if (chestForwardLeanEnabled)
        {
            LastForwardLean = calcForwardLean;
        }
        else if (!forwardLeanTransitionRunning)
        {
            LastForwardLean = riderDefaultForwardLean;
        }

        animator.SetFloat("ForwardLean", LastForwardLean);
    }
    void UpdateChestTwistValues()
    {
        float yVector = 180;
        if (exteneralTarget != null)
        {
            yVector = (GetLookAtYVector(exteneralTarget.position, rider.transform.position));
            yVector = LimitTwist(yVector);
        }
        actualChestTwist = Mathf.Lerp(actualChestTwist, yVector, chestTwistSpeed*Time.deltaTime);
    }
    void SetChestTwist()
    {
        animator.SetFloat("ChestTwist", actualChestTwist);
    }
    void AdjustRiderPosition()
    {
        rider.transform.localPosition = Vector3.zero;
        rider.transform.Translate(new Vector3(riderXPositionAdjustment, riderHeightAdjustment+1.6f, riderSaddlePosition));
        SetRiderLegSpread();
        SetFootPosition();
        SetHandPosition();
        SetRiderForwardLean();
    }

    Vector3 GetHeadLookAtPosition()
    {
        Vector3 returnValue = GetMidpoint(humanHeadIKTarget.position, staticHumanHeadIKTarget.position);
        returnValue.y += defaultHeadTilt;
        return returnValue;
    }
    Vector3 GetMidpoint(Vector3 position1, Vector3 position2)
    {
        return (position1 + position2) / 2;
    }
    bool RiderHasIKTargets()
    {
        if (rightLegIKTarget == null) return false;
        if (leftLegIKTarget == null) return false;
        if (rightHandIKTarget == null) return false;
        if (leftHandIKTarget == null) return false;
        return true;
    }
    Vector3 GetExternalTargetPosition()
    {  
        //Return HeadLookAt
        return exteneralTarget.position + new Vector3(0,externalTargetYOffset,0);
    }
    float LimitTwist(float yVector)
    {
        if (HandIKPassEnabled)
        {
            return Mathf.Clamp(yVector, 0 + (180 - maxChestTwistWithHandIKEnabled), 360 - (180 - maxChestTwistWithHandIKEnabled));
        }
        else
        {
            return Mathf.Clamp(yVector, 0 + (180 - maxChestTwist), 360 - (180 - maxChestTwist));
        }
    }
    float GetLookAtYVector(Vector3 position, Vector3 target)
    {
        Quaternion lookRotation = Quaternion.LookRotation(target - position);
        return lookRotation.eulerAngles.y;
    }

    IEnumerator DisableHandIKOverTime(float time)
    {
        float newWeight = 1f;// - time / Time.deltaTime;
        while (newWeight != 0)
        {
            newWeight = Mathf.Clamp01(newWeight-(Time.deltaTime/time));
            handIKWeight = newWeight;
            yield return 0;
        }
        HandIKPassEnabled = false;
    }
    IEnumerator DisableChestForwardLeanOverTime(float time)
    {
        forwardLeanTransitionRunning = true;
        chestForwardLeanEnabled = false;
        float newWeight = 1;
        while (newWeight != 0)
        {
            newWeight = Mathf.Clamp01(newWeight - (Time.deltaTime / time));
            LastForwardLean = Mathf.Clamp(newWeight * calcForwardLean, riderDefaultForwardLean, 0.5f);
            yield return 0;
        }
        forwardLeanTransitionRunning = false;
    }
    IEnumerator EnableHandIKOverTime(float time)
    {
        handIKWeight = 0;
        HandIKPassEnabled = true;
        float newWeight = 0f;// - time / Time.deltaTime;
        while (newWeight != 1)
        {
            newWeight = Mathf.Clamp01(newWeight + (Time.deltaTime / time));
            handIKWeight = newWeight;
            yield return 0;
        }
        
    }
    IEnumerator EnableChestForwardLeanOverTime(float time)
    {
        forwardLeanTransitionRunning = true;
        float newWeight = 0;
        while (newWeight != 1)
        {
            newWeight = Mathf.Clamp01(newWeight + (Time.deltaTime / time));
            LastForwardLean = Mathf.Clamp(newWeight * calcForwardLean, riderDefaultForwardLean, 0.5f);
            yield return 0;
        }
        chestForwardLeanEnabled = true;
        forwardLeanTransitionRunning = false;
    }

}
