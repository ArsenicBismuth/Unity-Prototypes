using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriftController : MonoBehaviour {

    #region Parameters
    public float Accel = 25.0f;         // In meters/second2
    public float Accel2 = 40.0f;
    public float TopSpeed = 100.0f;     // In meters/second
    public float Jump = 3.0f;           // In meters/second2
    public float GripX = 3.0f;          // In meters/second2
    public float GripZ = 0.5f;          // In meters/second2
    public float Rotate = 360.0f;       // In degree/second
    public float RotVelocity = 0.8f;    // Ratio of forward velocity transfered on rotation

    // Ground & air angular drag
    // reduce stumbling time on ground but maintain on-air one
    public float AngDragG = 5.0f;
    public float AngDragA = 0.05f;
    #endregion

    #region Intermediate
    private Rigidbody rigidbody;
    private Collider boxCollider;
    private float distToGround;

    // The actual value to be used (modification of parameters)
    private float rotate;
    private float accel;
    private float gripX;
    private float gripZ;

    // For determining drag direction
    private float isRight = 1.0f;
    private float isForward = 1.0f;
    private float isCW = 1.0f;

    private bool isMoving = false;
    private bool isRotating = false;
	private bool isBoost = false;
    private bool isGrounded = true;
    private bool isTouch = false;
    private bool isStumbling = false;

    private Vector3 rot = new Vector3(0f,0f,0f);   // Euler angles, value to set transform.eulerAngles
    private Vector3 drot = new Vector3(0f,0f,0f);  // Euler angles, value add to transform.eulerAngles
    #endregion

    // Use this for initialization
    void Start () {
		rigidbody = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        distToGround = boxCollider.bounds.extents.y;   // Pivot to the outermost collider
    }

    // Update is called once per frame
    void Update() {
        Debug.DrawRay(transform.position, rigidbody.velocity / 2, Color.green);
    }

    // Update is called once multiple times per frame (according to physics setting)
    void FixedUpdate() {
        #region Situational Check
        rotate = Rotate;
        gripX = GripX;
        gripZ = GripZ;
        rigidbody.angularDrag = AngDragG;

        // Adjustment in slope
        accel = accel * Mathf.Cos(transform.localEulerAngles.x * Mathf.Deg2Rad);
        accel = accel > 0f ? accel : 0f;
        gripZ = gripZ * Mathf.Cos(transform.localEulerAngles.x * Mathf.Deg2Rad);
        gripZ = gripZ > 0f ? gripZ : 0f;
        gripX = gripX * Mathf.Cos(transform.localEulerAngles.z * Mathf.Deg2Rad);
        gripX = gripX > 0f ? gripX : 0f;

        // A short raycast to check below
        isGrounded = Physics.Raycast(transform.position, -transform.up, distToGround + 0.1f);
        if (!isGrounded) {
            rotate = 0f;
            accel = 0f;
            gripX = 0f;
            gripZ = 0f;
            rigidbody.angularDrag = AngDragA;
        }

        // Prevent the rotational input intervenes with physics angular velocity 
        isStumbling = rigidbody.angularVelocity.magnitude > 0.1f * Rotate * Time.deltaTime;
        if (isStumbling) {
            //rotate = 0f;
        }

        //anim.SetBool("isMoving", isMoving);
        #endregion

        #region Inputs
        isMoving = false;

        if (Input.GetAxisRaw("Throttle") > 0.5f || Input.GetAxisRaw("Throttle") < -0.5f) {
            rigidbody.velocity += transform.forward * Input.GetAxisRaw("Throttle") * accel * Time.deltaTime;
            gripZ = 0f;     // Remove straight grip if wheel is rotating
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            rigidbody.velocity += transform.up * accel * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.R)) {  // Reset
            transform.eulerAngles = new Vector3(0,0,0);
            transform.position += Vector3.up * 2;
        }

        if (Input.GetAxisRaw("Sprint") > 0f) {
            accel = Accel2;
            isBoost = true;
        } else if (Input.GetAxisRaw("Sprint") < 1f) {
            accel = Accel;
            isBoost = false;
        }

        // Get the local-axis velocity before rotation (+x, +y, and +z = right, up, and forward)
        Vector3 pvel = transform.InverseTransformDirection(rigidbody.velocity);

        isRotating = false;

        // Turn by keyboard
        if (Input.GetAxisRaw("Sideways") > 0.5f || Input.GetAxisRaw("Sideways") < -0.5f) {
            isCW = Input.GetAxisRaw("Sideways");
            rotate_grad_key();
        }

        // Turn by facing cursor
        // Get the Screen positions of the object
        Vector2 objOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        // Get the Screen position of the mouse
        Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        // Get the angle between the points (absolute goal) = right (target) - left
        float angle = -AngleOffset(Angle2Points(objOnScreen, mouseOnScreen), -90.0f);

        //rotate_instant(angle);    // Rotation instant
        //rotate_grad_abs(angle);   // Rotation gradual - Absolute target
        //rotate_grad_rel(angle);   // Rotation gradual - Relative target
        #endregion

        #region Passives
        // Get the local-axis velocity after rotation
        Vector3 vel = transform.InverseTransformDirection(rigidbody.velocity);

        // Rotate the velocity vector
        // TODO: Tweak more, still feels strange
        //vel = pvel;                   // Transfer all
        if (isRotating) vel = vel * 0.5f + pvel * 0.5f; // Partial transfer

        // Sideway grip
        isRight = vel.x > 0f ? 1f : -1f;
        vel.x -= isRight * gripX * Time.deltaTime;   // Accelerate in opposing direction
        if (vel.x * isRight < 0f) vel.x = 0f;       // Check if changed polarity

        // Straight grip
        isForward = vel.z > 0f ? 1f : -1f;
        vel.z -= isForward * gripZ * Time.deltaTime;
        if (vel.z * isForward < 0f) vel.z = 0f;

        // Top speed
        if (vel.z > TopSpeed) vel.z = TopSpeed;
        else if (vel.z < -TopSpeed) vel.z = -TopSpeed;

        rigidbody.velocity = transform.TransformDirection(vel);
        #endregion

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

    #region Rotation methods
    /* Advised to not read eulerAngles, only write: https://answers.unity.com/questions/462073/
     * As it turns out, the problem isn't there. */

    /* As is: Conflict with physical Y-axis rotation, must be disabled.
     * Current methods:
     * 1. Prevent rotational input when there's angular velocity.
     * 2. Significantly increase angular drag while grounded.
     * 3. Result: rotation responding to environment, responsive input, & natural stumbling.
     */

    void rotate_instant(float angle) {
        if (rotate > 0f) {
            rot = transform.eulerAngles;
            rot.y = angle;
            transform.eulerAngles = rot;
            isRotating = true;
        }
    }

    void rotate_grad_key() {
        // Delta = right(taget) - left(current)
        drot.y = isCW * rotate * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        isRotating = true;
    }

    void rotate_grad_abs(float angle) {
        // Delta = right(taget) - left(current)
        rot = transform.eulerAngles;
        rot.y = AngleOffset(rot.y, 0f);

        float delta = Mathf.DeltaAngle(rot.y, angle);
        isCW = delta > 0f ? 1f : -1f;
        rot.y += isCW * rotate * Time.deltaTime;
        rot.y = AngleOffset(rot.y, 0f);

        delta = Mathf.DeltaAngle(AngleOffset(rot.y, 0f), angle);
        if (delta * isCW < 0f) rot.y = angle;       // Check if changed polarity
        else isRotating = true;

        // You can't set them directly as it'll set x & z to zero
        // if you're not using eulerAngles.x & z.
        transform.eulerAngles = rot;
        //transform.rotation = Quaternion.AngleAxis(rot.y, Vector3.up);
        //rigidbody.MoveRotation(Quaternion.Euler(rot));
    }

    void rotate_grad_rel(float angle) {
        // Delta = right(taget) - left(current)
        rot = transform.eulerAngles;
        rot.y = AngleOffset(rot.y, 0f);

        float delta = Mathf.DeltaAngle(rot.y, angle);
        isCW = delta > 0f ? 1f : -1f;
        drot.y = isCW * rotate * Time.deltaTime;
        rot.y = AngleOffset(rot.y, 0f);

        delta = Mathf.DeltaAngle(AngleOffset(rot.y, drot.y), angle);
        if (delta * isCW < 0f) drot.y = 0;       // Check if changed polarity
        else isRotating = true;

        // Add the drot to current rotation
        transform.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        //rigidbody.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        //transform.Rotate(drot, Space.Self);
        //rigidbody.AddTorque(drot);
        //rigidbody.MoveRotation(rigidbody.rotation * Quaternion.Euler(drot));
    }
    #endregion
}
