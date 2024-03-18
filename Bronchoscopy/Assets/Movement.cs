using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    
    public float speed = 0.1f;
    public float lookSpeed = 1f;
    public Joystick joystickMove;
    public Joystick joystickCam;
    
    private CharacterController character;
    private Vector3 trans;
    private Vector2 rot = Vector2.zero;
    
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
    }
    
    void FixedUpdate()
    {
        // Rotate according to mouse or right joystick, FPS
        float vertical, horizontal;
        if (joystickCam.Horizontal != 0 || joystickCam.Vertical != 0) {
            vertical = -joystickCam.Vertical;
            horizontal = joystickCam.Horizontal;
        } else {
            vertical = -Input.GetAxis("Mouse Y");
            horizontal = Input.GetAxis("Mouse X");
        }
        rot.y += horizontal;
        rot.x += vertical;
        rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        
        // Spaceship, full xy axis rotation
        transform.eulerAngles = new Vector2(rot.x, rot.y) * lookSpeed;  // Rotate in y & x axis, spaceship

        // Typical FPS, rotate in y-axis only (xy for camera)
        // rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        // transform.eulerAngles = new Vector2(0, rot.y) * lookSpeed;
        // Camera.main.transform.localRotation = Quaternion.Euler(rot.x * lookSpeed, 0, 0);

        // Movement either WASD or joystick
        float strafe, forward;
        if (joystickMove.Horizontal != 0 || joystickMove.Vertical != 0) {
            forward = joystickMove.Vertical;
            strafe = joystickMove.Horizontal;
        } else {
            forward = Input.GetAxis("Vertical");
            strafe = Input.GetAxis("Horizontal");
        }

        // Move
        Vector3 direction = new Vector3(strafe, 0, forward);
        Vector3 movement = transform.TransformDirection(direction);
        character.Move(movement * Time.deltaTime * speed);
    }
}
