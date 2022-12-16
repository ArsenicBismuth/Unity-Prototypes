using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public ShotData shot;   // The selected shot

    // Shot UI
    public Text shotTxt;
    private int ishot = 0;   // Index for changing in-game

    // Intermediatery
    private float scale = 0;    // Sphere size

    // Start is called before the first frame update
    void Start()
    {
        shotTxt.text = shot.Name;
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
        clone.name = shot.Name;
        clone.tag = "BallDynamic";

        lastSpawn = Time.time;
        master.spawn++;

        // Reset indicator size
        sphere.transform.localScale = new Vector3(0, 0, 0);
        scale = 0;

    }

    // Incrementally change the shot type
    public void ChangeShot() {
        ishot++;
        if (ishot >= master.Shots.Count) ishot = 1;

        shot = master.Shots[ishot];
        shotTxt.text = shot.Name;
        Master.Log("Shot", shot.Name);
    }
}
