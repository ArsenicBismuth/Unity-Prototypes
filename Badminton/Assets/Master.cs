using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Master : MonoBehaviour
{

    public Text debugTxt;
    public Head racket;
    private int fps;
    
    // Will be updated by latest ball being hit
    public float ballSpd = 0;
    
    private float headSpd = 0;
    private int reset = 3*90;
    private int iter = 0;

    void Start()
    {
        Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(90);
    }
    
    void Update()
    {
        // Update GUI
        fps = (int)(1f / Time.unscaledDeltaTime);
        debugTxt.text = fps +"\n"+ ballSpd +"\n"+ headSpd;
        
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
