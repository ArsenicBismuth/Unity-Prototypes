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
    public Cam hmd;
    public GameObject court;
    private int fps;        // Visual
    private int timestep;   // Physics

    private static Text consoleLog;

    // Intermediate
    private static GameObject _court;
    
    // Stats of latest data
    public float ballSpd = 0;
    private float headSpd = 0;

    private int reset = 3*90;
    private int iter = 0;

    // Toggle
    public bool spawner = false;
    public bool enemyMove = true;
    public bool data = true;
    private GameObject[] statics;

    // Scoring
    public int spawn = 0;
    public int hit = 0;
    private float score = 0;

    // Gizmos
    public LineRenderer lineLaser;
    public LineRenderer lineSpeed;
    public LineRenderer lineCurve;

    // GUI
    public UIMarker hitMarker;

    // Database - Shot types
    public List<ShotData> Shots;

    void Awake() {
        _court = court;
    }

    void Start()
    {
        // References: https://docs.unity3d.com/Packages/com.unity.xr.oculus@3.0/api/Unity.XR.Oculus.html
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(RefreshRate);
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = ResScale;

        // Limit to this FPS on editor
        #if UNITY_EDITOR
            Application.targetFrameRate = RefreshRate;
        #endif

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

        debugTxt.text = fps.ToString("000") +"\n"+
            pfps +" "+ timestep.ToString("0.00") +"\n"+
            ballSpd.ToString("0.00") +"\n"+
            headSpd.ToString("0.00") +"\n\n"+
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
    
    public void SetMovement(bool val) {
        enemyMove = val;
    }

    public void ToggleMovement() {
        enemyMove = !enemyMove;
    }

    // Utility
    public static void ToggleObject(GameObject obj) {
        if (obj.activeSelf) obj.SetActive(false);
        else obj.SetActive(true);
    }

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

    // Court
    public static bool CourtOut(Vector3 position, bool single = true) {
        Vector3 center = _court.transform.position;
        Vector3 pos = position-center;

        if (single) {
            if (Mathf.Abs(pos.x) > 5.18/2 || Mathf.Abs(pos.z) > 13.4/2) return true;
            else return false;
        } else {
            if (Mathf.Abs(pos.x) > 6.1/2 || Mathf.Abs(pos.z) > 13.4/2) return true;
            else return false;
        }
    }
}
