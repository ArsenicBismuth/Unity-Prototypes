using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMarker : MonoBehaviour
{
    public Image mark;

    // Get hit position relative to racket center and draw the mark there
    public void Mark(Vector3 pos) {
        Image hit = Instantiate(mark, transform.position, mark.transform.rotation);
        hit.transform.parent = gameObject.transform;
        hit.transform.localPosition = new Vector3(-pos.x/3, pos.y/3, 0);
        hit.transform.localScale = mark.transform.localScale;
    }

}
