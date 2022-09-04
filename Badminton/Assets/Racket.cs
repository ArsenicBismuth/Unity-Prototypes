using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Racket : MonoBehaviour
{
    
    Rigidbody rb;
    public Transform controller;
    
    private Vector3 posOffset;
    private Quaternion rotOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        // Automatically get offset from scene
        posOffset = transform.position - controller.position;
        rotOffset = transform.rotation * Quaternion.Inverse(controller.rotation);
    }

    // Update is called once per physic frame
    void FixedUpdate()
    {
        // Follow physically the controller, with certain angle
        rb.MovePosition(controller.position + controller.rotation * posOffset);
        rb.MoveRotation(controller.rotation * rotOffset);

    }
}
