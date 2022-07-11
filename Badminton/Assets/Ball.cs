using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    public Master master;
    public float speed;
    public Vector3 dir;
    
    private Vector3 dir2;   // dir w/o y-axis rot or yaw (z=0)
    private Quaternion yaw; // up or y-axis rotation

    public LineRenderer lineLaser;
    public LineRenderer lineSpeed;
    public LineRenderer lineCurve;
    
    
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

        // Get 2D vector by removing yaw (y-axis rotation)
        float ydeg = -Mathf.Rad2Deg*Mathf.Atan(dir.z/dir.x);
        yaw = Quaternion.Euler(0, ydeg, 0);     // Get the yaw
        dir2 = Quaternion.Inverse(yaw) * dir;   // Remove yaw
        
        DrawStraight();
        DrawCurve();
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

    private float CurveX(float t) {
        float x = speed * dir2.x * t;
        return x;
    }

    private float CurveY(float t) {
        float y = (float)(speed * dir2.y * t + 0.5 * Physics.gravity.y * t*t);
        return y;
    }
    
    private void DrawCurve()
    {
        // Set ball as origin
        Vector3 startPos = transform.position;

        // Delta time for each point
        lineCurve.positionCount = 100;
        float dt = (20f/speed)/100;  // Set for 100pts within 20m

        // float dt = 24f/160;  // Set so that 160m/s has 24pts
        // Debug.Log(dt);

        var points = new Vector3[lineCurve.positionCount];

        // Create point
        for (int i = 0; i < lineCurve.positionCount; i++) {
            float x = CurveX(i*dt);
            float y = CurveY(i*dt);
            points[i] = startPos + yaw * new Vector3(x, y, 0);
        }
        lineCurve.SetPositions(points);
    }
    
    // Not visible in-game
    private void OnDrawGizmos() 
    {
        // Debug show hit line
        Debug.DrawRay(transform.position, dir, Color.white);
    }
}
