using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RRSetIK : StateMachineBehaviour
{
    [SerializeField] bool handIKIsEnabled = false;
    [SerializeField] bool forwardLeanIsEnabled = true;
    bool setOnEnter = true;
    bool setOnExit = false;
    [SerializeField] float stateChangeTime= 2;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!setOnEnter) return;
        animator.gameObject.GetComponent<OnAnimatorIKRelay>().Saddle.SetHandIKPassEnabled(handIKIsEnabled,  stateChangeTime);
        animator.gameObject.GetComponent<OnAnimatorIKRelay>().Saddle.SetChestForwardLeanEnabled(forwardLeanIsEnabled, stateChangeTime);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!setOnExit) return;
        animator.gameObject.GetComponent<OnAnimatorIKRelay>().Saddle.SetHandIKPassEnabled(handIKIsEnabled, stateChangeTime);
        animator.gameObject.GetComponent<OnAnimatorIKRelay>().Saddle.SetChestForwardLeanEnabled(forwardLeanIsEnabled, stateChangeTime);
    }
}
