using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Instruction : MonoBehaviour
{

    public GameObject player;

    private Vector3 initPosition;
    private Text text;
    private Color initColor;
    private float alpha = 1f;

    // Start is called before the first frame update
    void Start()
    {
        initPosition = player.transform.position;
        text = GetComponent<Text>();
        initColor = text.color;
    }

    // Update is called once per frame
    void Update()
    {
        // If the player moved, fade out the instruction
        if (player.transform.position != initPosition)
        {
            alpha -= 0.01f;
            text.color = new Color(initColor.r, initColor.g, initColor.b, alpha);

            if (alpha <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
