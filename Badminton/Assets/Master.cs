using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Master : MonoBehaviour
{

    public int RefreshRate = 90;
    public float ResScale = 2.0f;

    public Text debugTxt;
    public Text consoleTxt;
    public Head racket;
    private int fps;        // Visual
    private int timestep;   // Physics

    private static Text consoleLog;
    
    // Will be updated by latest ball being hit
    public float ballSpd = 0;
    
    private float headSpd = 0;
    private int reset = 3*90;
    private int iter = 0;

    // Toggle
    public bool spawner = false;
    public bool staticBall = true;
    public bool enemyMove = true;
    private GameObject[] statics;

    // Scoring
    public int spawn = 0;
    public int hit = 0;
    private float score = 0;

    // Database - Shot types
    public List<ShotData> Shots;

    void Start()
    {
        // References: https://docs.unity3d.com/Packages/com.unity.xr.oculus@3.0/api/Unity.XR.Oculus.html
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(RefreshRate);
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = ResScale;

        // Find all static balls on start, can't use "Find" on inactive obj
        statics = GameObject.FindGameObjectsWithTag("BallStatic");

        consoleLog = consoleTxt;
    }
    
    void Update()
    {
        // Update GUI
        int fps = (int)(1f / Time.unscaledDeltaTime);
        float timestep = Time.fixedDeltaTime;
        float pfps = (int)(1f / timestep);

        if (spawn > 0) score = hit*100/spawn;

        debugTxt.text = fps +"\n"+
            pfps +" "+ timestep +"\n"+
            ballSpd +"\n"+ headSpd +"\n\n"+
            score+"%";

        iter++;
    }
    
    void FixedUpdate()
    {
        // Log fastest within X seconds
        if (racket.speed > headSpd)
            headSpd = racket.speed;
        
        // Reset fastest
        if (iter > reset) {
            headSpd = 0;
            iter = 0;
        }
    }

    // Toggles
    public void ToggleSpawner() {
        spawner = !spawner;

        // Reset if it's restarting
        if (spawner) {    
            spawn = 0;
            hit = 0;
            score = 0;
        }
    }

    public void ToggleStatic() {
        staticBall = !staticBall;
        
        // Toggle objects
        foreach (GameObject ball in statics) {
            ball.SetActive(staticBall);
        }
    }
    
    public void SetMovement(bool val) {
        enemyMove = val;
    }

    public void ToggleMovement() {
        enemyMove = !enemyMove;
    }

    // Utility
    public static void Log(params object[] a)
    {
        // General printing routine like Javascript, but on canvas
        var s = a[0].ToString();
        for ( int i = 1; i < a.Length; i++ ) {
            s += " ";
            s += a[i].ToString();
        }

        // Print also in debug
        Debug.Log(s);

        // Pseudo buffer: Append text to newline
        consoleLog.text = consoleLog.text + '\n' + s;

        if (consoleLog.text.Split('\n').Length > 5) {
            string[] lines = consoleLog.text
                .Split('\n')
                .Skip(1)
                .ToArray();
            consoleLog.text = string.Join('\n', lines);
        };
    }
}
