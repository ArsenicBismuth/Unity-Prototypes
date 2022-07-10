using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    
    public float speed;
    public Vector3 dir;
    private Vector3 oldPos;
    
    // Start is called before the first frame update
    void Start()
    {
        oldPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // Get speed
        Vector3 diff = transform.position - oldPos;
        speed = diff.magnitude / Time.deltaTime;
        oldPos = transform.position;
        
        // Get direction. Ignore slice: Always head's dir
        if (Vector3.Dot(transform.forward, diff) >= 0)
            // Stroke is along head's normal
            dir = transform.forward;
        else
            // Stroke is opposing head's normal
            dir = -1 * transform.forward;
    }
}
