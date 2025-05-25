using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class HeadCollider : MonoBehaviour
{
    
    // Proper hit area, 0.5 for 1 total (the original size is 2x the valid area)
    [Tooltip("Valid area, checked after hitting the collider. We only care about the XY area not Z since collider already ensures it's in the correct Z")]
    public Bounds valid;    // Due to the out-of-sync between logic & physics, it's common that a valid hit is thought as invalid.
    

    // Check a position if it's within valid hit zone
    public (bool, Vector3) CheckHit(Vector3 contact) {
        
        // Check contact in head's local transform, determine validity
        Vector3 relative = transform.InverseTransformPoint(contact);
        bool inside = valid.Contains(relative);
        
        // Neutralize against scaling
        relative = Vector3.Scale(relative, transform.localScale);

        return (inside, relative);
    }

    public Vector3 GetDir() {
        // Get from parent
        return transform.parent.GetComponent<Head>().dir;
    }

    public float GetSpeed() {
        // Get from parent
        return transform.parent.GetComponent<Head>().speed;
    }
}
