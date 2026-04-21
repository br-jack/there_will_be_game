using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnAnimatorIKRelay : MonoBehaviour
{
    public ReactiveRider Saddle = null;
    private void OnAnimatorIK(int layerIndex)
    {
        Saddle.OnRelayedAnimatorIK();
    }
}
