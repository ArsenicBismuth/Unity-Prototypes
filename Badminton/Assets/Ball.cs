using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    public Master master;
    public Vector3 dir;
    public float speed;
    public LineRenderer lineLaser;
    public LineRenderer lineSpeed;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // Collision with a trigger. Works on kinematic
    private void OnTriggerEnter(Collider other)
    {
        // Get racket head info
        dir = other.gameObject.GetComponent<Head>().dir;
        speed = other.gameObject.GetComponent<Head>().speed;
        master.ballSpd = speed; // Update debug info
        
        DrawStraight();
    }
    
    private void DrawStraight()
    {
        float length = 20;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (length * dir);
        
        // Direct projection
        lineLaser.SetPosition(0, startPos);
        lineLaser.SetPosition(1, endPos);
        
        // Intensity line
        Vector3 endPos2 = startPos + (speed/10 * dir);
        lineSpeed.SetPosition(0, startPos);
        lineSpeed.SetPosition(1, endPos2);
    }
    
    // Not visible in-game
    private void OnDrawGizmos() 
    {
        // Debug show hit line
        Debug.DrawRay(transform.position, dir, Color.white);
    }
}
