using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Button : MonoBehaviour
{

    public GameObject child;    // The visible object
    public UnityEvent onClick;  // The function to be called

    private float clickCD = 0.25f;
    private bool onHover;
    private float clicked;
    private Vector3 initPos;

    // Start is called before the first frame update
    void Start() {
        initPos = child.transform.position;
    }

    // Update is called once per frame
    void Update() {
        if (clicked + clickCD > Time.time) {
            child.transform.position = initPos + new Vector3(0, -0.15f, 0);
        } else if (onHover) {
            child.transform.position = initPos + new Vector3(0, -0.1f, 0);
        } else {
            child.transform.position = initPos;
        }

        // Keep onHover false by default, unless overwritten
        onHover = false;
    }

    void Hover() {
        onHover = true;
    }

    void Click() {
        onClick?.Invoke();
        clicked = Time.time;
    }
}
