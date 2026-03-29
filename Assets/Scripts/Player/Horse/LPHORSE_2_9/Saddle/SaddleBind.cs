using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaddleBind : MonoBehaviour
{
    [SerializeField] GameObject saddle = null;
    Transform chestBone;
    Transform saddleBone;

    // Start is called before the first frame update
    void Awake()
    {
        Transform[] childTransforms = gameObject.GetComponentsInChildren<Transform>();
        foreach (Transform t in childTransforms)
        {
            if (t.name == "Chest_Pull")
            {
                chestBone = t;
            }
            if (t.name == "Saddle_MCH_Back_end")
            {
                saddleBone = t;
            }
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        saddle.transform.position =  chestBone.position;
        saddle.transform.LookAt(saddleBone.position);
    }

}
