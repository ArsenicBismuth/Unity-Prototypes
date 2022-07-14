using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    public Master master;
    public GameObject contact;
    public static float terminal = 6.7f; // Terminal velocity, 6.51 to 6.87 m/s
    private float vtr = terminal;
    private float vt = terminal*terminal;

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
    // private void OnTriggerStay(Collider other)
    // {
    //     OnTriggerEnter(other);
    // }
    
    private void OnTriggerEnter(Collider other)
    {
        contact = other.gameObject;

        // Get racket head info
        dir = contact.GetComponent<Head>().dir;
        speed = contact.GetComponent<Head>().speed;
        master.ballSpd = speed; // Update debug info

        // Normalize to X+
        float ydeg = -Mathf.Rad2Deg*Mathf.Atan(dir.z/dir.x);
        if (dir.x < 0) ydeg -= 180;
        
        // Get 2D vector by removing yaw (y-axis rotation)
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
        Vector3 endPos2 = startPos + (speed/60 * dir);
        lineSpeed.SetPosition(0, endPos2 - 0.05f*dir);
        lineSpeed.SetPosition(1, endPos2);
    }

    private float g = -Physics.gravity.y;

    private float CurveX(float t) {
        float vx = speed * dir2.x;  // Initial
        float x = vt/g * Mathf.Log((vx*g*t + vt) / vt);
        return x;
    }

    private float CurveY(float x) {
        float vy = speed * dir2.y;  // Initial
        float vx = speed * dir2.x;
        float atan = Mathf.Atan(vtr/vy);

        float y = vt/g * Mathf.Log( Mathf.Sin( vtr * (Mathf.Exp(g*x/vt)-1) / vx + atan ) / Mathf.Sin(atan) );
        return y;
    }
    
    private float SimpleCurveX(float t) {
        float x = speed * dir2.x * t;
        return x;
    }

    private float SimpleCurveY(float t) {
        float y = (float)(speed * dir2.y * t + 0.5 * Physics.gravity.y * t*t);
        return y;
    }
    
    private void DrawCurve()
    {
        // Ref: https://www.ncbi.nlm.nih.gov/pmc/articles/PMC3761540/

        // Set ball as origin
        Vector3 startPos = transform.position;

        // Delta time for each point
        lineCurve.positionCount = 100;
        float dt = (80f/speed)/100;  // Set for 100pts within 80m

        // float dt = 24f/160;  // Set so that 160m/s has 24pts
        // Debug.Log(dt);

        var points = new Vector3[lineCurve.positionCount];

        // Create point
        for (int i = 0; i < lineCurve.positionCount; i++) {

            // Calculate x & y
            float x = CurveX(i*dt);
            float y = CurveY(x);
            points[i] = startPos + yaw * new Vector3(x, y, 0);

            // Hit ground
            if (points[i].y < 0) {
                lineCurve.positionCount = i+1;
            }
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
