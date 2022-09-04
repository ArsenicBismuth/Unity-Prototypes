using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Pointer : MonoBehaviour
{
    
    public XRNode inputSource;
    private InputDevice device;

    private struct Input { 
        public bool state, prev;
        public bool up, down;
    };
    private Input triggerButton;

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
        int layerMask = 1 << 7; // Bit shift to get a bit mask, only Button
        RaycastHit hit;

        // Does the ray intersect only object in that layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 50, layerMask)) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
            hit.transform.gameObject.SendMessage("Hover");

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
    }

}
