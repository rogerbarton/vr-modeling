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

            Native.InitializeNative(modelRoot, new NativeCallbacks.StringCallback(NativeCallbacks.DebugLog),
                new NativeCallbacks.VFCallback(NativeCallbacks.CreateMesh));
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
        public const string dllName = "libigl-interface";

        [DllImport(dllName)]
        public static extern void InitializeNative(string modelRootp, NativeCallbacks.StringCallback debugCallback,
            NativeCallbacks.VFCallback createMeshCallback);
        
        [DllImport(dllName)]
        public static extern int IncrementValue(int value);
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

        public delegate void VFCallback(Vector3[] V, int[] F);
        public static void CreateMesh(Vector3[] V, int[] F)
        {
            LibiglInterface.get.CreateMesh(V, F);
        }
    }
}