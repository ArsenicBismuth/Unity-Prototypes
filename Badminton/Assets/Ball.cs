using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody rb;
    public Master master;
    public GameObject contact;
    private GameObject child;

    // Parameters
    public float terminal = 6.8f; // Terminal velocity, 6.51 to 6.87 m/s
    private float vtr;          // terminal itself (alt: root)
    private float vt;           // terminal cubic (alt: terminal itself)

    // Physics, dynamic for itself
    public float moveSpd = -1;
    private Vector3 pos0;   // Initial position since launched
    private Vector3 move1;  // Forward without y
    private Vector3 move2;  // Dir w/o y-axis rot or yaw (z=0)
    private Vector3 prev;   // Prev pos, to get dir & rotate the mesh (child)

    
    private Vector3 dir;    // Direction, measured
    public float statSpd = 0; // Current speed, measured

    // Physics, static for gizmos
    public float speed;
    public Vector3 dir0;    // Racket head
    private Vector3 dir1;   // Forward without y
    private Vector3 dir2;   // Dir w/o y-axis rot or yaw (z=0)

    // Gizmos
    private LineRenderer lineLaser;
    private LineRenderer lineSpeed;
    private LineRenderer lineCurve;

    // Logics
    private float hitCD = 0.5f;
    private float lastHit = 0;  // Time of last hit
    private float init = 0;     // Time initialized

    // Audio
    public AudioClip audClip;
    float audVol = 0.5f;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        vtr = terminal;
        vt = terminal*terminal;

        lineLaser = master.lineLaser;
        lineSpeed = master.lineSpeed;
        lineCurve = master.lineCurve;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (moveSpd > 0) {

            // Get the visible part of object (child)
            child = gameObject.transform.GetChild(0).gameObject;

            init = Time.time;
            pos0 = transform.position;

            // Draw trajcetory on spawn
            // Draw(transform.forward, moveSpd);
            
            // Set direction, the formula uses x,y for z,y (squash xz => x)
            move1 = new Vector3(transform.forward.x, 0, transform.forward.z);
            move2 = new Vector3(move1.magnitude, transform.forward.y, 0);
            
            // Delete itself after few sec
            Destroy(gameObject, 2);

        }

    }

    void Update() {
        
    }

    // FixedUpdate to check collision better, behavior still eq to Update
    void FixedUpdate()
    {
        // This is dynamic ball
        if (moveSpd > 0) {

            // prev stores the position at the beginning of this FixedUpdate step
            prev = transform.position;

            float v0 = moveSpd;
            float Dt = Time.time - init;   // Time since start

            // Calculate absolute position relative to initial launch point pos0
            float x_component = CurveX(v0, Dt, move2);
            float y_component = CurveY(v0, x_component, move2);

            // Calculate the new absolute target position for the ball
            Vector3 newTargetPosition = pos0 + move1.normalized * x_component + Vector3.up * y_component;
            
            dir = newTargetPosition - prev;

            // Move the rigidbody to the new target position
            rb.MovePosition(newTargetPosition);
            
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1, Color.yellow);
            
            // Hit ground check should use the new target position
            if (newTargetPosition.y < 0) {

                // Out of bounds
                if (Master.CourtOut(newTargetPosition)) {
                    master.spawn -= 1;
                }

                Destroy(gameObject);
                return;
            }

            // Measure speed and update rotation
            if (dir.sqrMagnitude > Mathf.Epsilon) {
                statSpd = dir.magnitude / Time.fixedDeltaTime;
                child.transform.rotation = Quaternion.LookRotation(dir);    // Rotate the mesh
            } else {
                statSpd = 0;
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {

        contact = other.gameObject;

        // Hitting net
        if (contact.name == "Net") {
            master.spawn -= 1;
            Destroy(gameObject);
            return;
        }

        // Prevent multi contacts in short period
        if (lastHit + hitCD > Time.time) {
            return;
        }
        lastHit = Time.time;

        // Contact racket, check if in valid area
        HeadCollider racket = contact.GetComponent<HeadCollider>();
        if (!racket) return;

        // Mark the ball in head's local transform
        (bool valid, Vector3 relative) = racket.CheckHit(transform.position);
        master.hitMarker.Mark(relative, valid);

        // Valid, get hit info
        if (valid) Draw(racket.GetDir(), racket.GetSpeed());

        // For dynamic ones
        if (moveSpd > 0 && valid) {

            // Destroy on hit & add score
            master.hit += 1;
            AudioSource.PlayClipAtPoint(audClip, transform.position, audVol);
            Destroy(gameObject);

        }

    }

    private void Draw(Vector3 dir, float spd) {

        // Update public var
        dir0 = dir;
        speed = spd;
        master.ballSpd = speed; // Update debug info

        // Get direction, the formula uses x,y for z,y (squash xz => x)
        dir1 = new Vector3(dir0.x, 0, dir0.z);
        dir2 = new Vector3(dir1.magnitude, dir0.y, 0);
        
        DrawStraight();
        DrawCurve();
    }
    
    private void DrawStraight()
    {
        float length = 20;
        Vector3 startPos = Vector3.zero;
        Vector3 endPos = startPos + (length * dir0);
        
        // Direct projection
        lineLaser.transform.position = transform.position;
        lineLaser.SetPosition(0, startPos);
        lineLaser.SetPosition(1, endPos);

        
        // Intensity line - small strip of line
        Vector3 endPos2 = startPos + (speed/60 * dir0);
        lineSpeed.transform.position = transform.position;
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
        Vector3 startPos = Vector3.zero;
        lineCurve.transform.position = transform.position;

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
