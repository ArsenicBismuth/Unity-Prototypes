using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Head : MonoBehaviour
{
    
    public int clone = 5;

    [Tooltip("How long the clone lasts, means next frame after created")]
    public float cloneFrameDuration = 1;
    public float speed;
    public Vector3 dir;
    public bool master = true;

    // Proper hit area, 0.5 for 1 total, and z=10 coz we knew it's a hit from collider
    public Bounds valid;

    private Quaternion pRot;
    private Vector3 pPos;
    private bool firstMasterUpdateLogicRun = true;
    
    // Start is called before the first frame update
    void Start()
    {
        pPos = transform.position;
        pRot = transform.rotation;
    }

    void OnEnable()
    {
        if (master)
        {
            InputSystem.onAfterUpdate += MasterUpdateLogic;
            firstMasterUpdateLogicRun = true;
        }
    }

    void OnDisable()
    {
        if (master)
        {
            InputSystem.onAfterUpdate -= MasterUpdateLogic;
        }
    }

    void MasterUpdateLogic()
    {
        if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
        {
            if (firstMasterUpdateLogicRun)
            {
                firstMasterUpdateLogicRun = false;
            }

            Vector3 diff = transform.position - pPos;

            if (Time.deltaTime > 0.00001f)
            {
                speed = diff.magnitude / Time.deltaTime;
            } else {
                speed = 0f;
            }
            
            if (diff.sqrMagnitude > 0.00001f)
            {
                if (Vector3.Dot(transform.forward, diff.normalized) >= 0)
                    dir = transform.forward;
                else
                    dir = -1 * transform.forward;
            }

            if (diff.sqrMagnitude > 0.00001f) {
                int n = clone+1;
                for (int i=1; i<n; i++) {

                    Vector3 pos = Vector3.Slerp(pPos, transform.position, (float)i/n);
                    Quaternion rot = Quaternion.Slerp(pRot, transform.rotation, (float)i/n);

                    GameObject obj = Instantiate(gameObject, pos, rot);
                    Head head = obj.GetComponent<Head>();
                    head.master = false;
                    head.speed = speed;
                    head.dir = dir;
                }
            }

            pPos = transform.position;
            pRot = transform.rotation;
        }
    }

    // Update for clone-specific logic
    void Update()
    {
        // Only if a clone
        if (!master) {
            // Update dir to accurately reflect slerp
            // 'dir' was set by the master during instantiation
            if (Vector3.Dot(transform.forward, dir) >= 0)
                dir = transform.forward;
            else
                dir = -1 * transform.forward;

            // Remove itself after N frames (0 means next frame)
            if (cloneFrameDuration <= 0)
                Destroy(gameObject);
            cloneFrameDuration--;
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
