using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGraph2 : MonoBehaviour
{

    public Master master;

    // Similar to UIGraph, but 3D (xy position, z value)
    public Cam cam;

    public int N = 100;
    private Vector3[] points;

    // Data range for normalization
    public Vector3 minIn;
    public Vector3 maxIn;

    // Maximum size from line renderer's 2nd point
    public Vector3 maxOut;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        maxOut = line.GetPosition(1);

        points = new Vector3[N];

        line.positionCount = N;
        line.SetPositions(points);
    }

    void Update() {
        if (!master.data) return;
        
        float value = cam.speed;
        AddPoint(cam.transform.position, value);
    }

    public void AddPoint(Vector3 pos, float data)
    {
        // Keep X shift Y (Z = 0)
        for (int i=0; i<N-1; i++) {
            points[i] = points[i+1];
        }

        // Position XZ = graph XY, data = graph Z
        points[N-1].x = Map(pos.x, minIn.x, maxIn.x, -maxOut.x, maxOut.x);
        points[N-1].y = Map(pos.z, minIn.y, maxIn.y, 0, maxOut.y);
        points[N-1].z = Map(data, minIn.z, maxIn.z, 0, maxOut.z);
        line.SetPositions(points);
    }

    
    public static float Map(float value, float from1, float to1, float from2, float to2) {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
