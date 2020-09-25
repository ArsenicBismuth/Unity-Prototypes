using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CarChecks : MonoBehaviour
{
    public List<Transform> checkpoints = new List<Transform>();
    public int check = 0;   // Current checkpoint
    public int lap = 0;     // Current lap
    public float threshold = 4f; // Minimum distance to be counted
    
    public UnityEvent checkReach;

    float mindist = 99;

    // Update is called once per frame
    void Update()
    {

        // If given the option between Raycast and Distance, use Distance.
        // Because it doesn't use physics, simple root math.
        float dist = Vector3.Distance(checkpoints[check].position, transform.position);

        mindist = Mathf.Min(mindist, dist);
        //Debug.Log(mindist);

        if (dist < threshold) {
            // Call that event
            checkReach.Invoke();
            check += 1;
            if (check >= checkpoints.Count) {
                check = 0;
                lap += 1;
            }

            mindist = 99;
        }

    }

    public void Restart() {
        check = 0;
        lap = 0;
    }
}
