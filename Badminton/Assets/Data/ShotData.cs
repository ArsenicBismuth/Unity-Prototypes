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
    [HideInInspector]
    public float target;                       // y-rot target

    public float GetSpeed() {
        return Random.Range(spdMin, spdMax);
    }
    
    public float GetLift() {
        return Random.Range(-upMax, -upMin);
    }

    public float GetSide() {
        return target + Random.Range(sideMin, sideMax);
    }

    public float GetHeight() {
        return Random.Range(hMin, hMax);
    }

    public float SetTarget( Transform source, Transform dest) {
        Quaternion delta = Quaternion.FromToRotation(source.forward, dest.position - source.position);
        target = delta.eulerAngles.y;
        return target;
    }

    public string PrintShot() {
        return Name + " " + Mathf.Round(target).ToString() + "\n";
    }

}
