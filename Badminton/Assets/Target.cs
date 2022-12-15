using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{

    public GameObject hand;
    public bool grab = false;

    private float clickCD = 0.25f;  // Cooldown so it'll "stuck" for a while after clicked
    private bool onHover;
    private float clicked;
    
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        // Move down and stay for a while after clicked
        if (grab) {
            DisableRagdoll();
            transform.position = hand.transform.position + Vector3.forward * 1;
        } else {
            EnableRagdoll();
        }

        // Keep onHover false by default, unless overwritten
        onHover = false;
    }

    void Hover() {
        onHover = true;
    }

    void Click() {
        grab = !grab;
        clicked = Time.time;
    }
    
    // Let the rigidbody take control and detect collisions.
    void EnableRagdoll()
    {
        rb.isKinematic = false;
        rb.detectCollisions = true;
    }

    // Let animation control the rigidbody and ignore collisions.
    void DisableRagdoll()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
    }
}
