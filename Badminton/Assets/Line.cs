using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    
    Ball ball;
    
    // Start is called before the first frame update
    void Start()
    {
        ball = GetComponent<Ball>();
    }

    // Update is called once per frame
    void Update()
    {
        var dir = ball.dir;
        var speed = ball.speed;
    }
}
