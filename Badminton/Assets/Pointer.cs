using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Pointer : MonoBehaviour
{

    // Parameters
    private int layerMask = 1 << 7; // Bit shift to get a bit mask, only Clickable
    private int distance = 10;
    
    public XRNode inputSource;
    private InputDevice device;

    private struct InputState { 
        public bool state, prev;
        public bool up, down;
    };
    private InputState triggerButton;
    private InputState primaryButton;

    // Line pointer
    LineRenderer line;
    Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(inputSource);

        // Line from itself (using local space)
        line = gameObject.GetComponent<LineRenderer>();
        points = new Vector3[2];
        points[0] = Vector3.zero;
        points[1] = new Vector3(0, 0, distance);
        line.SetPositions(points);
        line.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {

        triggerButton = CheckInput(UnityEngine.XR.CommonUsages.triggerButton, triggerButton);
        primaryButton = CheckInput(UnityEngine.XR.CommonUsages.primaryButton, primaryButton);
        
        // Raycast to interact with buttons
        RaycastHit hit;

        if (primaryButton.state) line.enabled = true;
        else line.enabled = false;

        // Does the ray intersect only object in that layer
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance, layerMask)) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

            hit.transform.gameObject.SendMessage("Hover", null, SendMessageOptions.DontRequireReceiver);
            // Master.Log("Hover", hit.transform.gameObject.name);

            // Click while on hover - Trigger button (index)
            if (triggerButton.up) {

                var ui = hit.transform.gameObject.GetComponent<UnityEngine.UI.Button>();
                if (ui != null) {
                    ui.onClick.Invoke();
                    Master.Log("Click UI", hit.transform.gameObject.name);
                } else {
                    hit.transform.gameObject.SendMessage("Click", null, SendMessageOptions.DontRequireReceiver);
                    Master.Log("Click", hit.transform.gameObject.name);
                }
                
            }
        }

    }

    InputState CheckInput(InputFeatureUsage<bool> input, InputState output) {

        // Try to check value
        bool state;
        if (device.TryGetFeatureValue(input, out state)) {
            // Store only if valid
            output.state = state;
        } else {
            // Master.Log("Input: Fail");
        }

        // Check state change
        if (output.state != output.prev) {
            if (output.state) output.down = true;
            else output.up = true;
        } else {
            output.up = false;
            output.down = false;
        }
        output.prev = output.state;

        // For debug, defaults to LCtrl or click
        if (Input.GetButton("Fire1")) output.up = true;

        return output;
    }

}
