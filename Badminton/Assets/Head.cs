using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Head : MonoBehaviour
{
    
    public int clone = 5;
    public float speed;
    public Vector3 dir;
    public bool master = true;

    // Proper hit area, 0.5 for 1 total, and z=10 coz we knew it's a hit from collider
    public Bounds valid;

    private Quaternion pRot;
    private Vector3 pPos;

    private int frame = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        pPos = transform.position;
        pRot = transform.rotation;
    }

    // Update because speed calc is wonky with FixedUpdate at low deltaTime
    void Update()
    {
        
        // Only if main object
        if (master) {
            // Get speed
            Vector3 diff = transform.position - pPos;
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

                    // Interpolate spherically between the two
                    Vector3 pos = Vector3.Slerp(pPos, transform.position, (float)i/n);
                    Quaternion rot = Quaternion.Slerp(pRot, transform.rotation, (float)i/n);

                    var obj = Instantiate(gameObject, pos, rot);
                    obj.GetComponent<Head>().master = false;
                    obj.GetComponent<Head>().speed = speed;
                    obj.GetComponent<Head>().dir = dir;
                }
            }

            pPos = transform.position;
            pRot = transform.rotation;

        // The clones
        } else {
            
            // Update dir to accurately reflect slerp
            if (Vector3.Dot(transform.forward, dir) >= 0)
                dir = transform.forward;
            else
                dir = -1 * transform.forward;

            // Remove itself in next frame (or instantly)
            if (frame >= 0)
                Destroy(gameObject);
            frame++;
        }

    }

    // Check a position if it's within valid hit zone
    public (bool, Vector3) CheckHit(Vector3 contact) {
        
        // Check contact in head's local transform, determine validity
        Vector3 relative = transform.InverseTransformPoint(contact);
        bool inside = valid.Contains(relative);
        
        // Neutralize against scaling
        relative = Vector3.Scale(relative, transform.localScale);

        return (inside, relative);
    }
}
