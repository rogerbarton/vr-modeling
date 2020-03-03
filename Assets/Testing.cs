using System;
using System.Collections;
using System.Collections.Generic;
using libigl;
using UnityEngine;

public class Testing : MonoBehaviour
{
    void Start()
    {
        LibiglInterface.CheckInitialized();
    }

    private int value = 0;
    void Update()
    {
        if(Input.anyKeyDown)
            Debug.Log(value = Native.IncrementValue(value));
        // if (Input.GetKeyDown(KeyCode.Space))
        //     Native.LoadMesh();
    }
}
