using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public Master master;
    public GameObject target;
    public GameObject sphere;
    public Ball ball;
    public float spawnCD = 0.4f;    // In seconds
    
    private float lastSpawn = 0;    // Time of last spawn
    
    // Custom shot, undefined by Master.cs
    public ShotData custom;

    // Shot selector
    [Dropdown("master.Shots", "Name")]
    public ShotData shot;  // The selected shot

    // Intermediatery
    private float scale = 0;    // Sphere size

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get custom shot if shot select name = "Custom"
        if (shot.Name == "Custom" || shot.Name == "") {
            shot = custom;
        }

        if (master.spawner) {
            // dScale = target scale / tick per CD = target / (dur / dt)
            scale += 0.1f / (spawnCD / Time.deltaTime);
            sphere.transform.localScale = new Vector3(scale, scale, scale);
        }

        // Check cooldown (in seconds)
        if ((lastSpawn + spawnCD > Time.time) || !master.spawner) {
            return;
        }

        // Target's position to set as y-rot reference
        Quaternion delta = Quaternion.FromToRotation(transform.forward, target.transform.position - transform.position);

        // Define the shot & parameters, in m/s & degrees
        float spd = shot.GetSpeed();
        float lift = shot.GetLift();                     // X-rot
        float side = shot.GetSide(delta.eulerAngles.y);  // Y-rot
        float height = shot.GetHeight();

        Quaternion dir = Quaternion.Euler( lift, side, 0);
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        // Instantiate the projectile at the position and rotation of this transform
        Ball clone;
        clone = Instantiate(ball, transform.position, transform.rotation * dir);
        clone.moveSpd = spd;
        clone.name = "Push";
        clone.tag = "BallDynamic";

        lastSpawn = Time.time;
        master.spawn += 1;

        // Reset indicator size
        sphere.transform.localScale = new Vector3(0, 0, 0);
        scale = 0;

    }
}
