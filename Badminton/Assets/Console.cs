using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Javascript-like Console.log
public class Console
{
    public static void Log(params object[] a)
    {
        var s =a[0].ToString();
        for ( int i = 1; i < a.Length; i++ ) {
            s += " ";
            s += a[i].ToString();
        }
        Debug.Log(s);
    }
}
