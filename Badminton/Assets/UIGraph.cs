using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIGraph : MonoBehaviour
{

    // Note, add component, not object (so must manually drag-drop component)
    public Component comp;
    public string param;

    public int N = 100;
    private Vector3[] points;

    // Data range for normalization
    public float max = 100;
    public float min = 0;

    // Maximum size from line renderer's 2nd point
    private float maxX;
    private float maxY;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        maxX = line.GetPosition(1).x;
        maxY = line.GetPosition(1).y;

        // Init X values
        points = new Vector3[N];
        for (int i=0; i<N; i++) {
            points[i].x = i * maxX/N;
        }

        line.positionCount = N;
        line.SetPositions(points);
    }

    void Update() {
        float value = (float) GetPropertyValue(comp, param);
        AddPoint(value);
    }

    // Get arbitrary property using reflection
    public object GetPropertyValue(object t, string propertyName) {
        object val = t.GetType().GetField(propertyName).GetValue(t);
        return val;
    }

    public void AddPoint(float data)
    {
        // Keep X shift Y (Z = 0)
        for (int i=0; i<N-1; i++) {
            points[i].y = points[i+1].y;
        }
        points[N-1].y = Map(data);
        line.SetPositions(points);
    }

    float Map(float val) {
        if (val > max) val = max;
        else if (val < min) val = min;

        return (val - min) / (max - min) * (maxY - 0) + 0;
    }
}
