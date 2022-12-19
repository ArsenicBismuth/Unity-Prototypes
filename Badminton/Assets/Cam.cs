using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cam : MonoBehaviour
{
    public float speed;
    private Vector3 pPos = Vector3.zero;

    // Update is called once per frame
    void Update()
    {
        // Get speed
        Vector3 diff = transform.position - pPos;
        speed = diff.magnitude / Time.deltaTime;

        pPos = transform.position;
        
    }
}
