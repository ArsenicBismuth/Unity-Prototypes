using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;

public class Head : MonoBehaviour
{
    
    public int clone = 5;

    [Tooltip("How long the clone lasts. 0 means it's active for the frame it's spawned, then deactivates on the next frame's Update.")]
    public int cloneFrameDuration = 1;
    public float speed;
    public Vector3 dir;
    public bool master = true;


    private Quaternion pRot;
    private Vector3 pPos;

    private Vector3 pColliderPos;
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

        pColliderPos = transform.GetChild(0).position;

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

            // Parameter of racket swing, based on racket head (this child)
            Vector3 colliderPos = transform.GetChild(0).position;
            Vector3 diff = colliderPos - pColliderPos;

            if (Time.deltaTime > 0.00001f) {
                speed = diff.magnitude / Time.deltaTime;
            } else {
                speed = 0f;
            }
            
            if (diff.sqrMagnitude > 0.00001f) {
                if (Vector3.Dot(transform.forward, diff.normalized) >= 0)
                    dir = transform.forward;
                else
                    dir = -1 * transform.forward;
            }

            // Positioning of clones, based on self
            Vector3 currentPosition = transform.position;
            Quaternion currentRotation = transform.rotation;

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

            pColliderPos = colliderPos;
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

    public void ToggleVisibility() {
        // Toggle visibility of the child
        bool visible = transform.GetChild(0).GetComponent<MeshRenderer>().enabled;
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = !visible;

        // Also do the clones
        foreach (Head clone in clonePool) {
            clone.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = !visible;
        }
    }
}
