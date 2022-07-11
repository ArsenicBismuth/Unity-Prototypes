using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Master : MonoBehaviour
{

    public int RefreshRate = 90;
    public float ResScale = 2.0f;

    public Text debugTxt;
    public Head racket;
    private int fps;        // Visual
    private int timestep;   // Physics
    
    // Will be updated by latest ball being hit
    public float ballSpd = 0;
    
    private float headSpd = 0;
    private int reset = 3*90;
    private int iter = 0;

    void Start()
    {
        // References: https://docs.unity3d.com/Packages/com.unity.xr.oculus@3.0/api/Unity.XR.Oculus.html
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(RefreshRate);
        UnityEngine.XR.XRSettings.eyeTextureResolutionScale = ResScale;
    }
    
    void Update()
    {
        // Update GUI
        int fps = (int)(1f / Time.unscaledDeltaTime);
        float timestep = Time.fixedDeltaTime;
        float pfps = (int)(1f / timestep);

        debugTxt.text = fps +"\n"+
            pfps +" "+ timestep +"\n"+
            ballSpd +"\n"+ headSpd;

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
}
