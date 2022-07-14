using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    
    public int clone = 5;
    public float speed;
    public Vector3 dir;
    private Vector3 oldPos;
    public bool master = true;

    private int frame = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        oldPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        // Only if main object (instance id = original id)
        if (master) {
            // Get speed
            Vector3 diff = transform.position - oldPos;
            speed = diff.magnitude / Time.deltaTime;
            
            // Get direction. Ignore slice: Always head's dir
            if (Vector3.Dot(transform.forward, diff) >= 0)
                // Stroke is along head's normal
                dir = transform.forward;
            else
                // Stroke is opposing head's normal
                dir = -1 * transform.forward;

            // Create duplicate of itself inbetween
            if (diff.magnitude > 0) {
                var n = clone+1;
                for (int i=1; i<n; i++) {
                    // Set position, inbetween
                    Vector3 pos = oldPos + i*diff/n;

                    // Look at diff direction
                    Quaternion rot = Quaternion.LookRotation(-diff, Vector3.up);
                    // This is functionally unnecessary, but for proper contact
                    GameObject racket = transform.parent.gameObject;
                    rot *= Quaternion.Euler(0, 0, racket.transform.rotation.x);
                    
                    var obj = Instantiate(gameObject, pos, rot);
                    obj.GetComponent<Head>().master = false;
                    obj.GetComponent<Head>().speed = speed;
                    obj.GetComponent<Head>().dir = dir;
                }
            }

            oldPos = transform.position;

        // The clones
        } else {
            // Remove itself in next frame (or instantly)
            if (frame >= 0)
                Destroy(gameObject);
            frame++;
        }

    }
}
