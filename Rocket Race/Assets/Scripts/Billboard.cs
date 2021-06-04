using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform theCamera;

    
    private void Awake()
    {
        theCamera = GameObject.Find("Main Camera").transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(theCamera);
        transform.Rotate(0, 180, 0);
    }
}
