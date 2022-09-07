using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShotData
{

    public string Name = "Shot";
    public float spdMin; public float spdMax;   // m/s
    public float upMin; public float upMax;     // deg, deviation from normal
    public float sideMin; public float sideMax; // deg, deviation from player's position
    public float hMin; public float hMax;       // contact point height

    public float GetSpeed() {
        return Random.Range(spdMin, spdMax);
    }
    
    public float GetLift() {
        return Random.Range(-upMax, -upMin);
    }

    public float GetSide( float target=0 ) {
        return target + Random.Range(sideMin, sideMax);
    }

    public float GetHeight() {
        return Random.Range(hMin, hMax);
    }

}
