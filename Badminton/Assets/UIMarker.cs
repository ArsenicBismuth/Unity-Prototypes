using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMarker : MonoBehaviour
{
    public GameObject racketUI;
    public Image mark;
    public Color colValid;

    private Vector3 scale;

    void Start()
    {
        scale = racketUI.transform.localScale;
    }

    // Get hit position relative to racket center and draw the mark there
    public void Mark(Vector3 pos, bool valid) {
        Image hit = Instantiate(mark, transform.position, mark.transform.rotation);
        hit.transform.parent = gameObject.transform;
        hit.transform.localPosition = new Vector3(-pos.x*scale.x, pos.y*scale.y, 0);
        hit.transform.localScale = mark.transform.localScale;
        hit.tag = "UIMark";

        if (valid) hit.color = colValid;
    }

    [ContextMenu("Clear marks")]
    public void Clear() {
        foreach (Transform child in transform) {
            if (child.tag == "UIMark") {
                Destroy(child.gameObject);
            }
        }
    }

}
