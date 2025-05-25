using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMarker : MonoBehaviour
{
    public GameObject racketUI;
    public Image mark;
    public Color colValid;
    public int maxMarks = 50;

    private Vector3 scale;
    private Queue<Image> marks = new Queue<Image>();

    void Start()
    {
        scale = racketUI.transform.localScale;
    }

    // Get hit position relative to racket center and draw the mark there
    public void Mark(Vector3 pos, bool valid) {
        Image hit;
        
        if (marks.Count >= maxMarks) {
            // Reuse oldest mark
            hit = marks.Dequeue();
            hit.gameObject.SetActive(true);
        } else {
            // Create new mark
            hit = Instantiate(mark, transform.position, mark.transform.rotation);
            hit.transform.parent = gameObject.transform;
            hit.transform.localScale = mark.transform.localScale;
            hit.tag = "UIMark";
        }

        // Update position and color
        hit.transform.localPosition = new Vector3(-pos.x*scale.x, pos.y*scale.y, 0);
        hit.color = valid ? colValid : mark.color;
        
        marks.Enqueue(hit);
    }

    [ContextMenu("Clear marks")]
    public void Clear() {
        foreach (Image mark in marks) {
            // Just disable the mark
            mark.gameObject.SetActive(false);
        }
        marks.Clear();
    }

}
