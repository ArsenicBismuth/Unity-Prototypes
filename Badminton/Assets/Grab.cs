using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Grab : MonoBehaviour
{
    // Handle grabbing an object, moving its parent (since the mesh/collider is often the child of the actual object)

    public GameObject hand;
    public bool grabbed = false;

    // The function to be called
    public UnityEvent onGrabEnter;
    public UnityEvent onGrabExit;

    private bool onHover;
    private float clicked;
    
    // Control Rigidbody if exist, otherwise set to static position similar to start
    private Rigidbody rb;
    private Vector3 initPos;
    private Transform target;

    // Click feedback
    private MeshRenderer renderer;
    private Material matOrig;
    public Material matClick;

    // Start is called before the first frame update
    void Start() {
        // Determine if we grab the object itself or its parent
        if (transform.parent != null) target = transform.parent;
        else target = transform;

        rb = GetComponent<Rigidbody>();
        if (!rb) initPos = target.transform.position;

        renderer = GetComponent<MeshRenderer>();
        matOrig = renderer.material;
    }

    // Update is called once per frame
    void Update() {
        // Follow hand with an offset
        if (grabbed) target.position = hand.transform.position + hand.transform.TransformDirection(Vector3.forward) * 1;

        if (onHover) renderer.material = matClick;
        else renderer.material = matOrig;

        // Keep onHover false by default, unless overwritten
        onHover = false;
    }

    void Hover() {
        onHover = true;
    }

    void Click() {
        grabbed = !grabbed;
        clicked = Time.time;

        // On state change
        if (grabbed) {
            onGrabEnter?.Invoke();
            if (rb) DisableRagdoll();
        } else {
            onGrabExit?.Invoke();
            if (rb) EnableRagdoll();
            else {
                // Keep position, but reset vertical
                target.position = new Vector3(target.position.x, initPos.y, target.position.z);
            }
        }
    }
    
    // Let the rigidbody take control and detect collisions.
    void EnableRagdoll()
    {
        rb.useGravity = true;
        // rb.isKinematic = false;
        // rb.detectCollisions = true;
    }

    // Let animation control the rigidbody and ignore collisions.
    void DisableRagdoll()
    {
        rb.useGravity = false;
        // rb.isKinematic = true;
        // rb.detectCollisions = false;
    }
}
