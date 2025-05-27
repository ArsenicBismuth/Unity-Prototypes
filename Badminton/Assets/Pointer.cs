using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // Added for new Input System

public class Pointer : MonoBehaviour
{

    public Master master;

    // Parameters
    [SerializeField] LayerMask layerMask;   // Raycast layers
    private int distance = 10;

    // New Input System Actions
    [Tooltip("Action for grab/click")]
    public InputActionReference primaryAction;
    public InputActionReference showLineAction;
    public InputActionReference toggleLogAction;

    // Line pointer
    public GameObject sprite;
    LineRenderer line;
    Vector3[] points;

    // Start is called before the first frame update
    void Start()
    {
        // Line from itself (using local space)
        line = gameObject.GetComponent<LineRenderer>();
        points = new Vector3[2];
        points[0] = Vector3.zero;
        points[1] = new Vector3(0, 0, distance);
        line.SetPositions(points);
        line.enabled = false;

        // Enable actions
        primaryAction.action.Enable();
        showLineAction.action.Enable();
        toggleLogAction.action.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        // Raycast to interact with buttons
        RaycastHit hit;

        if (showLineAction.action.IsPressed()) line.enabled = true;       // Show line
        else line.enabled = false;

        if (toggleLogAction.action.WasReleasedThisFrame()) master.data = !master.data;   // Pause/unpause data logging

        // Does the ray intersect object in specific layers
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distance, layerMask)) {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);

            // Hover - send message & draw line till contact, draw sprite
            hit.transform.gameObject.SendMessage("Hover", null, SendMessageOptions.DontRequireReceiver);

            line.SetPosition(1, Vector3.forward * hit.distance);
            sprite.SetActive(true);
            sprite.transform.position = hit.point;

            // Click while on hover - Trigger button (index)
            if (primaryAction.action.WasReleasedThisFrame()) {

                var ui = hit.transform.gameObject.GetComponent<UnityEngine.UI.Button>();
                if (ui != null) {
                    ui.onClick.Invoke();
                    Master.Log("Click UI", hit.transform.gameObject.name);
                } else {
                    hit.transform.gameObject.SendMessage("Click", null, SendMessageOptions.DontRequireReceiver);
                    Master.Log("Click", hit.transform.gameObject.name);
                }
                
            }
        } else {

            // Draw line till max
            line.SetPosition(1, Vector3.forward * distance);
            sprite.SetActive(false);

        }

    }

    void LateUpdate()
    {
        if (sprite.activeSelf) {
            sprite.transform.forward = Camera.main.transform.forward;
        }
    }

}
