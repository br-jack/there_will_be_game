using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class getSpeed : MonoBehaviour
{
     public float speed;
     [SerializeField] private Animator anim;
    private Rigidbody rigidBody;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody>();
        
    }
    // Update is called once per frame
    void Update()
    {
        anim.SetFloat("Speed", math.sqrt( math.square(rigidBody.linearVelocity.x) + math.square(rigidBody.linearVelocity.z)) );
    }
}
