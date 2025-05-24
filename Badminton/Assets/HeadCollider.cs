using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class HeadCollider : MonoBehaviour
{
    
    // Proper hit area, 0.5 for 1 total (the original size is 2x the valid area)
    public Bounds valid;
    

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
