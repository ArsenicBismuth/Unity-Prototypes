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
    public Transform source;
    [HideInInspector]
    public Transform target;

    public float GetSpeed() {
        return Random.Range(spdMin, spdMax);
    }
    
    public float GetLift() {
        return Random.Range(-upMax, -upMin);
    }

    public float GetSide() {
        return CalculateAngle() + Random.Range(sideMin, sideMax);
    }

    public float GetHeight() {
        return Random.Range(hMin, hMax);
    }

    public void SetTarget( Transform source, Transform dest) {
        this.source = source;
        this.target = dest;
    }

    private float CalculateAngle() {
        // y-rot angle based on source & target vectors
        Quaternion delta = Quaternion.FromToRotation(source.forward, target.position - source.position);
        return delta.eulerAngles.y;
    }

    public string PrintShot() {
        return Name + " " + Mathf.Round(CalculateAngle()).ToString() + "\n";
    }

}
