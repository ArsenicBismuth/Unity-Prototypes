using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSwing : MonoBehaviour
{

    public Vector3 swingStartEuler;
    public Vector3 swingEndEuler;

    public float swingFrequency = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Swing start => end => start for every 1/frequency seconds
        float time = Time.time;
        float swingTime = time * swingFrequency;
        float swingProgress = swingTime % 1.0f; // Get the decimal part of the time
        
        // Convert progress to ping-pong between 0 and 1
        if (swingProgress > 0.5f)
        {
            swingProgress = 1 - (swingProgress - 0.5f) * 2; // Return from end to start
        }
        else
        {
            swingProgress *= 2; // Go from start to end
        }
        
        // Convert euler angles to quaternion
        Quaternion swingStartQuat = Quaternion.Euler(swingStartEuler);
        Quaternion swingEndQuat = Quaternion.Euler(swingEndEuler);
        transform.localRotation = Quaternion.Slerp(swingStartQuat, swingEndQuat, swingProgress);
    }
}
