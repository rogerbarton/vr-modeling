using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using libigl.rendering;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace libigl
{
    public class LibiglInterface : MonoBehaviour
    {
        public static LibiglInterface get;

        public string modelRoot;
        public GameObject baseModel;

        void Awake()
        {
            if (!get) get = this;
            else
            {
                Debug.LogError("A Statics instance already exists.");
                Destroy(this);
            }
            DontDestroyOnLoad(gameObject);

            if (modelRoot == null)
                modelRoot = Application.dataPath + "/Models/";

            //TODO: need use fixed or pin callbacks?
            Native.InitializeNative(modelRoot, NativeCallbacks.DebugLog, NativeCallbacks.CreateMesh);
        }

        public static void CheckInitialized()
        {
            if (!get)            //load static scene with the instance scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("StaticScene", LoadSceneMode.Additive);
        }

        public void CreateMesh(Vector3[] V, int[] F)
        {
            LibiglMeshFilter meshFilter = Instantiate(baseModel).GetComponent<LibiglMeshFilter>();
            meshFilter.UpdateMeshFilter(V, F);
        }
    }

    
    /// <summary>
    /// C# to C++ Communication
    /// Contains all C++ function declarations
    /// </summary>
    public static class Native
    {
        const string dllName = "libigl-interface";

        [DllImport(dllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern void InitializeNative(
            [In] string modelRootp, 
            [In] NativeCallbacks.StringCallback debugCallback,
            [In] NativeCallbacks.VFCallback createMeshCallback);
        
        [DllImport(dllName, ExactSpelling = true)]
        public static extern int IncrementValue([In, Out]int value);
        
        [DllImport(dllName, ExactSpelling = true)]
        public static extern int LoadMesh(string value);

        [DllImport(dllName, ExactSpelling = true)]
        public static extern unsafe void FillMesh(void* VPtr, int VSize, void* FPtr, int FSize);

        
        [DllImport(dllName, ExactSpelling = true)]
        public static extern void MoveV([In, Out] IntPtr VArr, int VSize, [In] float[] directionArr);

        [DllImport(dllName, ExactSpelling = true)]
        public static extern void ComputeColors(
            [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[,] outColors, int outColorsSize,
            [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[,] Vptr, int VSize,
            int nV);
    }
    

    /// <summary>
    /// C++ to C# Communication
    /// Contains all callbacks from the native context
    /// </summary>
    public static class NativeCallbacks
    {
        //Based on https://answers.unity.com/questions/30620/how-to-debug-c-dll-code.html
        public delegate void StringCallback(string message);
        public static void DebugLog(string message)
        {
            Debug.Log("[c++] " + message);
        }

        public delegate void VFCallback([In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[,] V, int vLength, int[] F);
        public static void CreateMesh(float[,] V, int vLength, int[] F)
        {
            // LibiglInterface.get.CreateMesh(V, F);
        }
    }
}