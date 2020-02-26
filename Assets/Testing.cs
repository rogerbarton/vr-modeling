using System;
using System.Collections;
using System.Collections.Generic;
using libigl;
using UnityEngine;

public class Testing : MonoBehaviour
{
    void Start()
    {
        try
        {
            LibiglInterface.Initialize();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    void Update()
    {
        if(Input.anyKeyDown)
            Debug.Log(LibiglInterface.IncrementValue(0));
    }
}
