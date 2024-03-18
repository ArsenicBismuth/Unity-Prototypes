using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    
    public float speed = 0.1f;
    public float lookSpeed = 1f;
    
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
        // Rotate according to mouse, FPS
        rot.y += Input.GetAxis("Mouse X");
        rot.x += -Input.GetAxis("Mouse Y");
        rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        
        // Spaceship, full xy axis rotation
        transform.eulerAngles = new Vector2(rot.x, rot.y) * lookSpeed;  // Rotate in y & x axis, spaceship

        // Typical FPS, rotate in y-axis only (xy for camera)
        // rot.x = Mathf.Clamp(rot.x, -90f, 90f);
        // transform.eulerAngles = new Vector2(0, rot.y) * lookSpeed;
        // Camera.main.transform.localRotation = Quaternion.Euler(rot.x * lookSpeed, 0, 0);

        // Move
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 movement = transform.TransformDirection(direction);
        character.Move(movement * Time.deltaTime * speed);
    }
}
