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

    // Ground & air angular drag
    // reduce stumbling time on ground but maintain on-air one
    public float AngDragG = 5.0f;
    public float AngDragA = 0.05f;

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
    private bool isStumbling = false;

    private Vector3 rot = new Vector3(0f,0f,0f);   // Euler angles, value to set transform.eulerAngles
    private Vector3 drot = new Vector3(0f,0f,0f);  // Euler angles, value add to transform.eulerAngles

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
        rigidBody.angularDrag = AngDragG;

        //accel = accel * Mathf.Cos(transform.localEulerAngles.z * Mathf.Deg2Rad);
        //accel = accel > 0f ? accel : 0f;
        //grip = grip * Mathf.Cos(transform.localEulerAngles.x * Mathf.Deg2Rad);
        //grip = grip > 0f ? grip : 0f;
        Debug.Log(transform.localEulerAngles.z + " " + accel);
        
        // A short raycast to check below
        isGrounded = Physics.Raycast(transform.position, -transform.up, distToGround + 0.1f);
        if (!isGrounded) {
            rotate = 0f;
            accel = 0f;
            grip = 0f;
            rigidBody.angularDrag = AngDragA;
        }

        // Prevent the rotational input intervenes with physics angular velocity 
        isStumbling = rigidBody.angularVelocity.magnitude > 0.1f * Rotate * Time.deltaTime;
        if (isStumbling) {
            rotate = 0f;
        }

        //anim.SetBool("isMoving", isMoving);


        // Get the Screen positions of the object
        Vector2 objOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        // Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        // Get the angle between the points (absolute goal) = right (target) - left
        float angle = -AngleOffset(Angle2Points(objOnScreen, mouseOnScreen), -90.0f);

        // Rotation instant
        //if (rotate > 0f) {
        //    rot = transform.eulerAngles;
        //    rot.y = angle;
        //    transform.eulerAngles = rot;
        //}

        /* Advised to not read eulerAngles, only write: https://answers.unity.com/questions/462073/
         * As it turns out, the problem isn't there. */

        /* As is: Conflict with physical Y-axis rotation, must be disabled.
         * Current methods:
         * 1. Prevent rotational input when there's angular velocity.
         * 2. Significantly increase angular drag while grounded.
         * 3. Result: rotation responding to environment, responsive input, & natural stumbling.
         */

        // Rotation gradual - Absolute target
        // Delta = right(taget) - left(current)
        rot = transform.eulerAngles;
        rot.y = AngleOffset(rot.y, 0f);
        //Debug.Log("--- " + rot.y);

        float delta = Mathf.DeltaAngle(rot.y, angle);
        isCW = delta > 0f ? 1f : -1f;
        rot.y += isCW * rotate * Time.deltaTime;
        rot.y = AngleOffset(rot.y, 0f);
        //Debug.Log(rot.y);

        delta = Mathf.DeltaAngle(AngleOffset(rot.y, 0f), angle);
        if (delta * isCW < 0f) rot.y = angle;       // Check if changed polarity

        // You can't set them directly as it'll set x & z to zero
        // if you're not using eulerAngles.x & z.
        transform.eulerAngles = rot;
        //transform.rotation = Quaternion.AngleAxis(rot.y, Vector3.up);
        //rigidBody.MoveRotation(Quaternion.Euler(rot));
        //Debug.Log(rot);


        // Rotation gradual - Relative target
        // Delta = right(taget) - left(current)
        //rot = transform.eulerAngles;
        //rot.y = AngleOffset(rot.y, 0f);
        //Debug.Log("--- " + rot.y);

        //float delta = Mathf.DeltaAngle(rot.y, angle);
        //isCW = delta > 0f ? 1f : -1f;
        //drot.y = isCW * rotate * Time.deltaTime;
        //rot.y = AngleOffset(rot.y, 0f);

        //delta = Mathf.DeltaAngle(AngleOffset(rot.y, drot.y), angle);
        //if (delta * isCW < 0f) drot.y = 0;       // Check if changed polarity
        //Debug.Log(drot.y);

        //// Add the drot to current rotation
        //transform.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        ////rigidBody.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        ////transform.Rotate(drot, Space.Self);
        ////rigidBody.AddTorque(drot);
        ////rigidBody.MoveRotation(rigidBody.rotation * Quaternion.Euler(drot));
        //Debug.Log(rot);



        /// Passives
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
        raw = (raw + offset) % 360;             // Mod by 360, to not exceed 360
        if (raw > 180.0f) raw -= 360.0f;
        if (raw < -180.0f) raw += 360.0f;
        return raw;
    }
}
