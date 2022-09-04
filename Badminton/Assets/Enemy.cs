using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    
    public Master master;

    // Params: Range, relative
    public Bounds range;        // x = sides, z = face
    public float accel = 8;     // Launch only
    public float dist = 5;      // Max distance
    public bool baseReset = true;  // Move back to base or not
    
    // Game logics
    private Vector3 basePos;
    private Vector3 prevPos;
    private Vector3 goalPos;
    private bool onPos = true;
    private bool toBase = false;

    private float moveCD = 1;
    private float lastMove = 0;

    private Vector3 velocity;
    private CharacterController character;

    // Start is called before the first frame update
    void Start()
    {
        character = GetComponent<CharacterController>();

        // Base = start obj position
        basePos = transform.position;

        // Restate bounds as absolute position
        range.center = basePos;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = transform.position;
        Vector3 max;
        Vector3 min;

        if (onPos) {

            // Check cooldown (in seconds)
            if (lastMove + moveCD > Time.time) {
                return;
            }

            // Set target: delta not > max, and pos not > range
            max.x = pos.x + dist;
            if (max.x > range.max.x) max.x = range.max.x;
            
            min.x = pos.x - dist;
            if (min.x < range.min.x) min.x = range.min.x;
            
            max.z = pos.z + dist;
            if (max.z > range.max.z) max.z = range.max.z;
            
            min.z = pos.z - dist;
            if (min.z < range.min.z) min.z = range.min.z;

            // Store current position
            prevPos = transform.position;

            // Define goal position
            if (toBase) {
                goalPos = basePos;
                // Get random goal after this
                toBase = false;
            } else {
                goalPos.y = transform.position.y;
                goalPos.x = Random.Range(min.x, max.x);
                goalPos.z = Random.Range(min.z, max.z);

                // If reset, move to base after goal
                if (baseReset) {
                    toBase = true;
                }
            }

            velocity = new Vector3(0, 0, 0);

            // Start moving
            onPos = false;

        } else {

            // Get direction
            Vector3 dir = (goalPos - pos).normalized;
            
            // Launch into position
            velocity += dir * accel * Time.deltaTime;
            Vector3 ds = velocity * Time.deltaTime;

            // Check if it'd pass goal position
            if (PassGoal(pos + ds, goalPos, prevPos)) {
                onPos = true;
                lastMove = Time.time;
            } else {
                character.Move(ds);
            }

        }
    }

    bool PassGoal(Vector3 current, Vector3 target, Vector3 prev) {
        Vector3 now = target-current;
        Vector3 goal = target-prev;

        // Exceeding if dot product of "now" against "goal" is negative
        return (Vector3.Dot(now, goal) <= 0);
    }
}
