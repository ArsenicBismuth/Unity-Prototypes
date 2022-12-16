using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Pointer : MonoBehaviour
{

    // Parameters
    private int layerMask = 1 << 7; // Bit shift to get a bit mask, only Clickable
    private int distance = 5;
    
    public XRNode inputSource;
    private InputDevice device;

    private struct InputState { 
        public bool state, prev;
        public bool up, down;
    };
    private InputState triggerButton;

    // Start is called before the first frame update
    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(inputSource);
    }

    // Update is called once per frame
    void Update()
    {

        CheckInput();
        
        // Raycast to interact with buttons
        RaycastHit hit;

        // Does the ray intersect only object in that layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance, layerMask)) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            hit.transform.gameObject.SendMessage("Hover");
            // Master.Log("Hover", hit.transform.gameObject.name);

            // Click while on hover - Trigger button (index)
            if (triggerButton.up) {
                hit.transform.gameObject.SendMessage("Click");
            }
        }

    }

    void CheckInput() {

        // Try to check value
        bool state;
        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out state)) {
            // Store only if valid
            triggerButton.state = state;
        } else {
            // Master.Log("Input: Fail");
        }

        // Check state change
        if (triggerButton.state != triggerButton.prev) {
            if (triggerButton.state) triggerButton.down = true;
            else triggerButton.up = true;
        } else {
            triggerButton.up = false;
            triggerButton.down = false;
        }
        triggerButton.prev = triggerButton.state;

        // For debug, defaults to LCtrl or click
        if (Input.GetButton("Fire1")) triggerButton.up = true;
    }

}
