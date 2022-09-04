using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    
    public Master master;
    public GameObject player;
    public GameObject sphere;
    public Ball ball;
    public float spawnCD = 0.4f;    // In seconds
    
    private float lastSpawn = 0;    // Time of last spawn
    
    // Types of shots & its parameters
    [System.Serializable]
    public struct Shot { 
        public float spdMin; public float spdMax;   // m/s
        public float upMin; public float upMax;     // deg, deviation from normal
        public float sideMin; public float sideMax; // deg, deviation from player's position
    }
    public Shot push;

    private float scale = 0;    // Sphere size

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (master.spawner) {
            // dScale = target scale / tick per CD = target / (dur / dt)
            scale += 0.1f / (spawnCD / Time.deltaTime);
            sphere.transform.localScale = new Vector3(scale, scale, scale);
        }

        // Check cooldown (in seconds)
        if ((lastSpawn + spawnCD > Time.time) || !master.spawner) {
            return;
        }

        // Player's position to set as y-rot reference
        Quaternion delta = Quaternion.FromToRotation(transform.forward, player.transform.position - transform.position);

        // Define the shot & parameters, in m/s & degrees
        Shot shot = push;
        float spd = Random.Range(shot.spdMin, shot.spdMax);
        float lift = Random.Range(shot.upMin, shot.upMax);      // X-rot
        float side = Random.Range(delta.eulerAngles.y + shot.sideMin, delta.eulerAngles.y + shot.sideMax);  // Y-rot

        Quaternion dir = Quaternion.Euler( lift, side, 0);

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
