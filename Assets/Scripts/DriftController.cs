using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Global enum
public enum Faction {
    Player,
    Enemy,
    Neutral
};



public class DriftController : MonoBehaviour {

    #region Parameters
    public float Accel = 15.0f;         // In meters/second2
    public float Boost = 4f/3;          // In ratio
    public float TopSpeed = 20.0f;      // In meters/second
    public float Jump = 3.0f;           // In meters/second2
    public float GripX = 6.0f;          // In meters/second2
    public float GripZ = 3.0f;          // In meters/second2
    public float Rotate = 270.0f;       // In degree/second
    public float RotVel = 0.5f;         // Ratio of forward velocity transfered on rotation
    
    public Faction carFaction = Faction.Player;  // Drop-down to select faction of this object
    public Transform Target;            // Target for the AI to act upon

    // Ground & air angular drag
    // reduce stumbling time on ground but maintain on-air one
    float AngDragG = 5.0f;
    float AngDragA = 0.05f;

    float MinRotSpd = 2f;          // Speed to start rotating
    float MaxRotSpd = 6f;          // Speed to reach max rotation

    // AI-specific parameters
    [Header("AI Behaviors")]
    public float turnTh = 20f;      // Delta threshold to goal before start turning
    #endregion

    #region Intermediate
    Rigidbody rigidbody;
    Bounds groupCollider;
    float distToGround;

    // The actual value to be used (modification of parameters)
    float rotate;
    float accel;
    float gripX;
    float gripZ;

    // For determining drag direction
    float isRight = 1.0f;
    float isForward = 1.0f;
    float speed = 0f;

    bool isRotating = false;
    bool isGrounded = true;
    bool isStumbling = false;

    // Control signals
    float inThrottle = 0f;
    [HideInInspector] public float inTurn = 0f;
    bool inReset = false;
    bool inBoost = false;
    
    Vector3 vel = new Vector3(0f, 0f, 0f);
    Vector3 pvel = new Vector3(0f, 0f, 0f);
    #endregion



    // Use this for initialization
    void Start () {
		rigidbody = GetComponent<Rigidbody>();
        
        groupCollider = GetBounds(gameObject);     // Get the full collider boundary of group
        distToGround = groupCollider.extents.y;    // Pivot to the outermost collider

        //distToGround = transform.position.y + 1f;
    }

    // Update is called once per frame
    void Update() {
        Debug.DrawRay(transform.position, rigidbody.velocity / 2, Color.green);
    }

    // Update is called once multiple times per frame (according to physics setting)
    void FixedUpdate() {
        #region Situational Check
        accel = Accel;
        rotate = Rotate;
        gripX = GripX;
        gripZ = GripZ;
        rigidbody.angularDrag = AngDragG;

        // Adjustment in slope
        accel = accel * Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad);
        accel = accel > 0f ? accel : 0f;
        gripZ = gripZ * Mathf.Cos(transform.eulerAngles.x * Mathf.Deg2Rad);
        gripZ = gripZ > 0f ? gripZ : 0f;
        gripX = gripX * Mathf.Cos(transform.eulerAngles.z * Mathf.Deg2Rad);
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

        // Adjustment of angular velocity to speed magnitude
        if (speed < MinRotSpd) {
            rotate = 0f;
        } else {
            rotate = speed / MaxRotSpd * rotate;
            if (rotate > Rotate) rotate = Rotate;
        }

        //anim.SetBool("isMoving", isMoving);
        #endregion

        // Get command from keyboard or simple AI (conditional rulesets)
        switch(carFaction) {
            case Faction.Player:
                InputKeyboard();
                break;
            case Faction.Enemy:
                InputAI();
                break;
            default:
                // Do nothing
                break;
        }

        // Execute the commands
        Controller();


        #region Passives
        // Get the local-axis velocity after rotation
        vel = transform.InverseTransformDirection(rigidbody.velocity);

        // Rotate the velocity vector
        // TODO: Tweak more, still feels strange
        //vel = pvel;                   // Transfer all
        if (isRotating) {
            vel = vel * (1 - RotVel) + pvel * RotVel; // Partial transfer
            vel = vel.normalized * speed;
        }

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



    // Get input values from keyboard
    void InputKeyboard() {
        inThrottle = Input.GetAxisRaw("Throttle");
        inReset = Input.GetKeyDown(KeyCode.R);
        inBoost = Input.GetAxisRaw("Boost") > 0f;
        inTurn = Input.GetAxisRaw("Sideways");
    }

    void InputAI() {
        inThrottle = 1f;

        // Turn by facing player
        // Get the Screen positions of the this object & player
        //Vector2 thisOnScreen = Camera.main.WorldToViewportPoint(transform.position);
        //Vector2 mouseOnScreen = (Vector2)Camera.main.ScreenToViewportPoint(Input.mousePosition);
        //Vector2 targetOnScreen = Camera.main.WorldToViewportPoint(Target.position);
        //float angle = -AngleOffset(Angle2Points(thisOnScreen, targetOnScreen), -90.0f);

        // Turn by facing player
        // Get the angle between the points (absolute goal) = right (target) - left
        float angle = AngleOffset(Angle2Points(transform.position, Target.position), 0f);

        Vector3 rot = transform.eulerAngles;
        float delta = Mathf.DeltaAngle(rot.y, angle);
        //inTurn = delta > 0f ? 1f : -1f;

        if (delta > 10f) inTurn = 1f;
        else if (delta < -10f) inTurn = -1f;
        else inTurn = 0f;

        // TODO: Make functions below to be compatible with current system, only outputting "inTurn".
        //RotateInstant(angle);    // Rotation instant
        //RotateGradAbsolute(angle);   // Rotation gradual - Absolute target
        //RotateGradRelative(angle);   // Rotation gradual - Relative target
    }

    // Executing the queued inputs
    void Controller() {

        if (inBoost) accel *= Boost; // Higher acceleration

        if (inThrottle > 0.5f || inThrottle < -0.5f) {
            rigidbody.velocity += transform.forward * inThrottle * accel * Time.deltaTime;
            gripZ = 0f;     // Remove straight grip if wheel is rotating
        }

        if (inReset) {  // Reset
            transform.eulerAngles = new Vector3(0, 0, 0);
            transform.position += Vector3.up * 2;
        }
        
        isRotating = false;

        // Get the local-axis velocity before new input (+x, +y, and +z = right, up, and forward)
        pvel = transform.InverseTransformDirection(rigidbody.velocity);
        speed = pvel.magnitude;

        // Turn statically
        if (inTurn > 0.5f || inTurn < -0.5f) {
            RotateGradConst(inTurn);
        }
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

    Vector3 drot = new Vector3(0f, 0f, 0f);

    void RotateInstant(float angle) {
        if (rotate > 0f) {
            Vector3 rot = transform.eulerAngles;
            rot.y = angle;
            transform.eulerAngles = rot;
            isRotating = true;
        }
    }

    void RotateGradConst(float isCW) {
        // Delta = right(taget) - left(current)
        drot.y = isCW * rotate * Time.deltaTime;
        transform.rotation *= Quaternion.AngleAxis(drot.y, transform.up);
        isRotating = true;
    }

    void RotateGradAbsolute(float angle) {
        // Delta = right(taget) - left(current)
        Vector3 rot = transform.eulerAngles;
        rot.y = AngleOffset(rot.y, 0f);

        float delta = Mathf.DeltaAngle(rot.y, angle);
        float isCW = delta > 0f ? 1f : -1f;
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

    void RotateGradRelative(float angle) {
        // Delta = right(taget) - left(current)
        Vector3 rot = transform.eulerAngles;
        rot.y = AngleOffset(rot.y, 0f);

        float delta = Mathf.DeltaAngle(rot.y, angle);
        float isCW = delta > 0f ? 1f : -1f;

        // Value add to transform.eulerAngles
        drot.y = isCW * rotate * Time.deltaTime;
        drot.y = AngleOffset(rot.y, 0f);

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



    #region Utilities
    float Angle2Points(Vector3 a, Vector3 b) {
        //return Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg;
        return Mathf.Atan2(b.x - a.x, b.z - a.z) * Mathf.Rad2Deg;
    }

    float AngleOffset(float raw, float offset) {
        raw = (raw + offset) % 360;             // Mod by 360, to not exceed 360
        if (raw > 180.0f) raw -= 360.0f;
        if (raw < -180.0f) raw += 360.0f;
        return raw;
    }

    // Get bound of a large 
    public static Bounds GetBounds(GameObject obj) {

        // Switch every collider to renderer for more accurate result
        Bounds bounds = new Bounds();
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0) {

            //Find first enabled renderer to start encapsulate from it
            foreach (Collider collider in colliders) {

                if (collider.enabled) {
                    bounds = collider.bounds;
                    break;
                }
            }

            //Encapsulate (grow bounds to include another) for all collider
            foreach (Collider collider in colliders) {
                if (collider.enabled) {
                    bounds.Encapsulate(collider.bounds);
                }
            }
        }
        return bounds;
    }
    #endregion
}
