using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Movement : MonoBehaviour
{
    
    public float speed = 1;
    public XRNode inputSource;
    private InputDevice device;
    public Camera cam;
    
    private Vector2 inputAxis;
    private CharacterController character;

    private float g;
    
    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();
        device = InputDevices.GetDeviceAtXRNode(inputSource);
    }

    // Update is called once per frame
    void Update()
    {
        device.TryGetFeatureValue(CommonUsages.primary2DAxis, out inputAxis);
        
        if (character.isGrounded) g = 0;
        else g = Physics.gravity.y;

        // Move relative to head/camera direction
        Quaternion headYaw = Quaternion.Euler(0, cam.transform.eulerAngles.y, 0);
        Vector3 direction = headYaw * new Vector3(inputAxis.x, g / speed, inputAxis.y);
        character.Move(direction * Time.deltaTime * speed);
    }
}
