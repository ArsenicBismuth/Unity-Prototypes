using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Spawner : MonoBehaviour
{
    
    public Master master;
    public GameObject target;
    public GameObject targetPlan;
    public GameObject sphere;
    public Ball ball;
    public float spawnCD = 0.4f;    // Cooldown in seconds
    
    private float lastSpawn = 0;    // Time of last spawn

    // Shot selector
    [Dropdown("master.Shots", "Name")]
    public ShotData shot;   // The selected shot
    public List<ShotData> plan;
    private int iplan = 0;

    // Shot UI
    public Text shotTxt;
    public Text cooldownTxt;
    private int ishot = 0;   // Index for UI
    
    // Plan UI
    public Text planTxt;
    
    // Intermediatery
    private float scale = 0;    // Sphere size

    // Start is called before the first frame update
    void Start()
    {
        shotTxt.text = shot.Name;
        cooldownTxt.text = (spawnCD*10).ToString();
    }

    // Update is called once per frame
    void Update()
    {

        if (master.spawner && spawnCD > 0) {
            scale += 0.1f / (spawnCD / Time.deltaTime);
            sphere.transform.localScale = new Vector3(scale, scale, scale);
        }

        // Check cooldown (in seconds)
        if ((lastSpawn + spawnCD > Time.time) || !master.spawner) {
            return;
        }

        // Use plan if it exists
        if (plan.Count > 0) {
            shot = plan[iplan];
            iplan++;
            if (iplan >= plan.Count) iplan = 0;
        } else {
            // Target's position to set as y-rot reference, realtime for non-plan mode
            shot.SetTarget(transform, target.transform);
        }

        // Define the shot & parameters, in m/s & degrees
        float spd = shot.GetSpeed();
        float lift = shot.GetLift();  // X-rot
        float side = shot.GetSide();  // Y-rot
        float height = shot.GetHeight();

        Quaternion dir = Quaternion.Euler( lift, side, 0);
        transform.position = new Vector3(transform.position.x, height, transform.position.z);

        // Instantiate the projectile at the position and rotation of this transform
        Ball clone = Instantiate(ball, transform.position, transform.rotation * dir);
        clone.moveSpd = spd;
        clone.name = shot.Name;
        clone.tag = "BallDynamic";

        lastSpawn = Time.time;
        master.spawn++;

        // Reset indicator size
        sphere.transform.localScale = new Vector3(0, 0, 0);
        scale = 0;

    }

    // UI - Incrementally change the shot type
    public void ChangeShot() {
        ishot++;
        if (ishot >= master.Shots.Count) ishot = 1;

        shot = master.Shots[ishot];
        shotTxt.text = shot.Name;
        Master.Log("Shot", shot.Name);
    }
    
    // UI - Change frequency
    public void ChangeCD(float val) {
        spawnCD += val;
        if (spawnCD < 0) spawnCD = 0;

        cooldownTxt.text = Mathf.Round(spawnCD*10).ToString();
        Master.Log("CD", spawnCD);
    }

    // UI - Add plan based on current shot (menu) & target
    public void AddPlan() {
        ShotData select = master.Shots[ishot];
        
        // Create new target cylinder
        GameObject targetClone = Instantiate(targetPlan, target.transform.position, target.transform.rotation);
        select.SetTarget(transform, targetClone.transform);
        plan.Add(select);
        iplan = 0;
        PrintPlan();
    }

    public void RemovePlan() {
        plan.RemoveAt(plan.Count - 1);
        iplan = 0;
        PrintPlan();
    }

    public void PrintPlan() {
        string text = "";
        foreach (var shot in plan) {
            text += shot.PrintShot();
        }
        planTxt.text = text;
    }
}
