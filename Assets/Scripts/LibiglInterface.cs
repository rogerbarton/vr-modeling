using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace libigl
{
    public static class LibiglInterface
    {
        public static string modelRoot;

        public static void Initialize()
        {
            if (modelRoot == null)
                modelRoot = Application.dataPath + "/Models/";
            //Call cpp init with model root
            Debug.Log(IncrementValue(0));
            InitializeNative(new DebugCallback(DebugLog));
        }

        [DllImport("libigl-interface")]
        public static extern int IncrementValue(int value);
        
        
        
        
        
        // C++ to C# Communication
        //Based on https://answers.unity.com/questions/30620/how-to-debug-c-dll-code.html
        public delegate void DebugCallback(string message);

        [DllImport("libigl-interface")]
        public static extern void InitializeNative(DebugCallback callback);

        public static void DebugLog(string message)
        {
            Debug.Log("c++ " + message);
        }

    }
}