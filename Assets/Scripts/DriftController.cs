using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftController : MonoBehaviour {
    
    public float Accel = 25.0f;     // In meters/second2
    public float Accel2 = 40.0f;
    public float TopSpeed = 100.0f; // In meters/second
    public float Jump = 3.0f;       // In meters/second2
    public float Grip = 3.0f;       // In meters/second2
    public float Rotate = 360.0f;   // In degree/second

    private Rigidbody rigidBody;
    private Collider boxCollider;
    private float distToGround;

    private float rotate;
    private float accel;
    private float grip;

    private float isRight = 1.0f;
    private float isCW = 1.0f;

    private bool isMoving = false;
	private bool isBoost = false;
    private bool isGrounded = true;
    private bool isTouch = false;

    // Use this for initialization
    void Start () {
		rigidBody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        distToGround = boxCollider.bounds.extents.y;   // Pivot to the outermost collider
    }

    // Update is called once per frame
    void Update() {

    }

    // Update is called once multiple times per frame (according to physics setting)
    void FixedUpdate() {
        /// Inputs
        isMoving = false;

        if (Input.GetAxisRaw("Throttle") > 0.5f || Input.GetAxisRaw("Throttle") < -0.5f) {
            rigidBody.velocity += transform.forward * Input.GetAxisRaw("Throttle") * accel * Time.deltaTime;
            isMoving = true;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            //transform.Translate(Vector3.up * Accel * Time.deltaTime);
            rigidBody.velocity += transform.up * accel * Time.deltaTime;
        }

        if (Input.GetAxisRaw("Sprint") > 0f) {
            accel = Accel2;
            isBoost = true;
        } else if (Input.GetAxisRaw("Sprint") < 1f) {
            accel = Accel;
            isBoost = false;
        }

        rotate = Rotate;
        grip = Grip;

        // A short raycast to check below
        isGrounded = Physics.Raycast(transform.position, -transform.up, distToGround + 0.1f);
        if (!isGrounded) {
            rotate = 0f;
            accel = 0f;
            grip = 0f;
        }

        //anim.SetBool("isMoving", isMoving);



        /// Passives
        // Get the Screen positions of the object
        Vector2 objOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        // Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        // Get the angle between the points (absolute goal) = right (target) - left
        float angle = -AngleOffset(Angle2Points(objOnScreen, mouseOnScreen), -90.0f);

        // Rotation instant
        //transform.rotation = Quaternion.Euler(new Vector3(0f, angle, 0f));

        // Rotation gradual
        Vector3 rot = transform.rotation.eulerAngles;

        // Delta = right (taget) - left (current)
        float delta = Mathf.DeltaAngle(AngleOffset(rot.y, 0f), angle);
        Debug.Log(angle + " " + (rot.y - 180f) + " " + delta);
        isCW = delta > 0f ? 1f : -1f;
        rot.y += isCW * rotate * Time.deltaTime;    // Rotate in the goal direction

        delta = Mathf.DeltaAngle(AngleOffset(rot.y, 0f), angle);
        if (delta * isCW < 0f) rot.y = angle;       // Check if changed polarity
        
        //rigidBody.rotation = Quaternion.Euler(rot); // Physical transform
        transform.rotation = Quaternion.Euler(rot); // Animation transform

        //// Rotation gradual
        //Vector3 rot = transform.rotation.eulerAngles;
        //Vector3 drot = new Vector3(0, 0, 0);   // Angular velocity

        //// Delta = right (taget) - left (current)
        //float delta = Mathf.DeltaAngle(AngleOffset(rot.y, 0f), angle);
        //Debug.Log(angle + " " + (rot.y - 180f) + " " + delta);
        //isCW = delta > 0f ? 1f : -1f;
        //drot.y = isCW * rotate * Time.deltaTime;     // Rotate in the goal direction

        //delta = Mathf.DeltaAngle(AngleOffset(rot.y, drot.y), angle);
        //if (delta * isCW < 0f) drot.y = delta;       // Check if changed polarity

        ////rigidBody.rotation = Quaternion.Euler(rot); // Physical transform
        ////transform.rotation = Quaternion.Euler(rot); // Animation transform
        //transform.Rotate(drot, Space.Self); // Animation transform


        // Get the local-axis velocity
        // +x, +y, and +z consitute to right, up, and forward
        Vector3 vel = transform.InverseTransformDirection(rigidBody.velocity);

        // Sideway grip
        isRight = vel.x > 0f ? 1f : -1f;
        vel.x -= isRight * grip * Time.deltaTime;   // Accelerate in opposing direction
        if (vel.x * isRight < 0f) vel.x = 0f;       // Check if changed polarity

        //// Backward grip
        //if (vel.z < -1f) {
        //    vel.z += grip * Time.deltaTime;
        //    if (vel.z > 0f) vel.z = 0f;
        //}

        // Top speed
        if (vel.z > TopSpeed) vel.z = TopSpeed;
        else if (vel.z < -TopSpeed) vel.z = -TopSpeed;

        rigidBody.velocity = transform.TransformDirection(vel);

    }

    float Angle2Points(Vector3 a, Vector3 b) {
        return Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
    }

    float AngleOffset(float raw, float offset) {
        raw += offset;
        if (raw > 180.0f) raw = -360.0f + raw;
        if (raw < -180.0f) raw = 360.0f + raw;
        return raw;
    }
}
