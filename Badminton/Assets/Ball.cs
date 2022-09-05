using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    
    public Master master;
    public GameObject contact;

    // Parameters
    public static float terminal = 6.7f; // Terminal velocity, 6.51 to 6.87 m/s
    private float vtr = terminal;           // vt root
    private float vt = terminal*terminal;   // terminal squared

    // Physics, dynamic for itself
    public float moveSpd = -1;
    private Vector3 pos0;   // Initial position since launched
    private Vector3 move1;  // Forward without y
    private Vector3 move2;  // Dir w/o y-axis rot or yaw (z=0)

    // Physics, static for gizmos
    public float speed;
    public Vector3 dir0;    // Racket head
    private Vector3 dir1;   // Forward without y
    private Vector3 dir2;   // Dir w/o y-axis rot or yaw (z=0)

    // Gizmos
    public LineRenderer lineLaser;
    public LineRenderer lineSpeed;
    public LineRenderer lineCurve;

    // Logics
    private float hitCD = 0.5f;
    private float lastHit = 0;  // Time of last hit
    private float init = 0;     // Time initialized

    // Audio
    public AudioClip audClip;
    float audVol = 0.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if (moveSpd > 0) {

            init = Time.time;
            pos0 = transform.position;
            
            // Set direction, the formula uses x,y for z,y (squash xz => x)
            move1 = new Vector3(transform.forward.x, 0, transform.forward.z);
            move2 = new Vector3(move1.magnitude, transform.forward.y, 0);
            
            // Delete itself after few sec
            Destroy(gameObject, 2);

        }

    }

    // Update is called once per frame
    void Update()
    {
        // This is dynamic ball
        if (moveSpd > 0) {

            float v0 = moveSpd;
            float Dt = Time.time - init;   // Time since start

            // Calculate relative position from pos0 (not speed)
            float x = CurveX(v0, Dt, move2);
            float y = CurveY(v0, x, move2);
            // float x = SimpleCurveX(v0, Dt, move2);
            // float y = SimpleCurveY(v0, Dt, move2);
            // Console.Log(x,y);

            // Set position forward by x, and up by y
            transform.position = pos0 + move1.normalized * x + Vector3.up * y;
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1, Color.yellow);
            
            // Hit ground
            if (transform.position.y < 0) {
                Destroy(gameObject);
            }

        }
    }
    
    private void OnTriggerEnter(Collider other)
    {

        if (moveSpd > 0) {

            // Destroy on hit & add score
            Destroy(gameObject);
            master.hit += 1;
            AudioSource.PlayClipAtPoint(audClip, transform.position, audVol);

        } else {

            // Check cooldown (in seconds)
            if (lastHit + hitCD > Time.time) {
                return;
            }

            contact = other.gameObject;

            // Get racket head info
            dir0 = contact.GetComponent<Head>().dir;
            speed = contact.GetComponent<Head>().speed;
            master.ballSpd = speed; // Update debug info
            
            // Get direction, the formula uses x,y for z,y (squash xz => x)
            dir1 = new Vector3(dir0.x, 0, dir0.z);
            dir2 = new Vector3(dir1.magnitude, dir0.y, 0);
            
            DrawStraight();
            DrawCurve();

            lastHit = Time.time;

        }
    }
    
    private void DrawStraight()
    {
        float length = 20;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (length * dir0);
        
        // Direct projection
        lineLaser.SetPosition(0, startPos);
        lineLaser.SetPosition(1, endPos);
        
        // Intensity line
        Vector3 endPos2 = startPos + (speed/60 * dir0);
        lineSpeed.SetPosition(0, endPos2 - 0.05f*dir0);
        lineSpeed.SetPosition(1, endPos2);
    }

    private float g = -Physics.gravity.y;

    private float CurveX(float v0, float t, Vector3 dir) {
        float vx = v0 * dir.x;  // Initial
        float x = vt/g * Mathf.Log((vx*g*t + vt) / vt);
        return x;
    }

    private float CurveY(float v0, float x, Vector3 dir) {
        float vy = v0 * dir.y;  // Initial
        float vx = v0 * dir.x;
        float atan = Mathf.Atan(vtr/vy);

        float y = vt/g * Mathf.Log( Mathf.Sin( vtr * (Mathf.Exp(g*x/vt)-1) / vx + atan ) / Mathf.Sin(atan) );
        return y;
    }
    
    private float SimpleCurveX(float v0, float t, Vector3 dir) {
        float x = v0 * dir.x * t;
        return x;
    }

    private float SimpleCurveY(float v0, float t, Vector3 dir) {
        float y = (float)(v0 * dir.y * t + 0.5 * Physics.gravity.y * t*t);
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
            float x = CurveX(speed, i*dt, dir2);
            float y = CurveY(speed, x, dir2);
            // float x = SimpleCurveX(speed, i*dt, dir2);
            // float y = SimpleCurveY(speed, i*dt, dir2);
            points[i] = startPos + dir1.normalized * x + Vector3.up * y;

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
        Debug.DrawRay(transform.position, dir0, Color.white);
        Debug.DrawRay(transform.position, dir1, Color.yellow);
    }
}
