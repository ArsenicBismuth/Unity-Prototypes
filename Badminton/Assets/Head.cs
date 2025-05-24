using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public class Head : MonoBehaviour
{
    
    public int clone = 5;

    [Tooltip("How long the clone lasts. 0 means it's active for the frame it's spawned, then deactivates on the next frame's Update.")]
    public int cloneFrameDuration = 1;
    public float speed;
    public Vector3 dir;
    public bool master = true;

    // Proper hit area, 0.5 for 1 total, and z=10 coz we knew it's a hit from collider
    public Bounds valid;

    private Quaternion pRot;
    private Vector3 pPos;
    private bool firstMasterUpdateLogicRun = true;
    
    // Object pooling for clones
    public int initialPoolSize = 10;
    private List<Head> clonePool;
    private int currentPoolIndex = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        pPos = transform.position;
        pRot = transform.rotation;

        if (master)
        {
            initialPoolSize = clone * (cloneFrameDuration + 1);
            InitializePool();
        }
    }

    void InitializePool()
    {
        clonePool = new List<Head>(initialPoolSize);
        // Ensure the original gameObject (master) has its Head component correctly configured before instantiating pool.
        // Its 'master' flag should be true.

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject obj = Instantiate(gameObject, transform.position, transform.rotation);
            Head pooledHead = obj.GetComponent<Head>();
            if (pooledHead != null)
            {
                pooledHead.master = false; // CRITICAL: Ensure pooled objects are slaves
                obj.SetActive(false); // Start inactive
                clonePool.Add(pooledHead);
            }
            else
            {
                Debug.LogError("Head.cs: Failed to get Head component on pooled clone instance. Destroying problematic instance.", obj);
                Destroy(obj); // Clean up malformed instance
            }
        }
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
        if (!master || clonePool == null || clonePool.Count == 0) // Guard clause for non-masters or uninitialized pool
            return;

        if (InputState.currentUpdateType == InputUpdateType.BeforeRender)
        {
            if (firstMasterUpdateLogicRun)
            {
                firstMasterUpdateLogicRun = false;
            }

            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;
            Vector3 diff = currentPosition - pPos;

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
                int n = clone + 1;
                for (int i = 1; i < n; i++) {
                    Vector3 pos = Vector3.Slerp(pPos, currentPosition, (float)i/n);
                    Quaternion rot = Quaternion.Slerp(pRot, currentRotation, (float)i/n);

                    if (clonePool.Count == 0) return; // Should not happen if initialized

                    Head pooledClone = clonePool[currentPoolIndex];
                    currentPoolIndex = (currentPoolIndex + 1) % clonePool.Count; // Cycle through the pool

                    GameObject cloneGO = pooledClone.gameObject;
                    cloneGO.transform.position = pos;
                    cloneGO.transform.rotation = rot;
                    
                    // Clone parameters match master's
                    pooledClone.speed = this.speed;
                    pooledClone.dir = this.dir;
                    pooledClone.cloneFrameDuration = this.cloneFrameDuration;
                    
                    cloneGO.SetActive(true);
                }
            }

            pPos = currentPosition;
            pRot = currentRotation;
        }
    }

    // Update for clone-specific logic
    void Update()
    {
        if (!master) {
            // Update dir to accurately reflect slerp for the clone itself
            if (Vector3.Dot(transform.forward, dir) >= 0)
                this.dir = transform.forward; // Clone updates its own dir based on its orientation
            else
                this.dir = -1 * transform.forward;

            cloneFrameDuration--; 
            if (cloneFrameDuration < 0) 
            {
                gameObject.SetActive(false);
            }
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
