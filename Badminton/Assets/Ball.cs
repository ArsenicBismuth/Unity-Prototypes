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

    // Physics, static for gizmos
    public float speed;
    public Vector3 dir0;
    
    private Vector3 dir2;   // dir w/o y-axis rot or yaw (z=0)
    private Quaternion yaw; // up or y-axis rotation

    // Gizmos
    public LineRenderer lineLaser;
    public LineRenderer lineSpeed;
    public LineRenderer lineCurve;

    // Logics
    private float hitCD = 0.5f;
    private float lastHit = 0;  // Time of last hit

    // Audio
    public AudioClip audClip;
    float audVol = 0.5f;
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // This is dynamic ball
        if (moveSpd > 0) {
            // Delete itself after few sec
            Destroy(gameObject, 2);

            // Set direction & speed (absolute coz Translate() will be local)
            Vector3 ballDir = Vector3.forward;

            // Translate relative to local space
            transform.Translate(ballDir * moveSpd * Time.deltaTime);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {

        // Check cooldown (in seconds)
        if (lastHit + hitCD > Time.time) {
            return;
        }

        contact = other.gameObject;

        // Get racket head info
        dir0 = contact.GetComponent<Head>().dir;
        speed = contact.GetComponent<Head>().speed;
        master.ballSpd = speed; // Update debug info

        // Normalize to X+
        float ydeg = -Mathf.Rad2Deg*Mathf.Atan(dir0.z/dir0.x);
        if (dir0.x < 0) ydeg -= 180;
        
        // Get 2D vector by removing yaw (y-axis rotation)
        yaw = Quaternion.Euler(0, ydeg, 0);     // Get the yaw
        dir2 = Quaternion.Inverse(yaw) * dir0;   // Remove yaw
        
        DrawStraight();
        DrawCurve();

        lastHit = Time.time;

        if (moveSpd > 0) {

            // Destroy on hit & add score
            Destroy(gameObject);
            master.hit += 1;
            AudioSource.PlayClipAtPoint(audClip, transform.position, audVol);

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
        Debug.DrawRay(transform.position, dir0, Color.white);
    }
}
