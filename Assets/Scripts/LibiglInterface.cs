using System;
using System.Runtime.InteropServices;
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

            if (string.IsNullOrEmpty(modelRoot))
                modelRoot = Application.dataPath + "/Models/";
            
            Debug.Log("Model Root: " + modelRoot);    
            //TODO: need use fixed or pin callbacks?
            Native.InitializeNative(modelRoot, NativeCallbacks.DebugLog, NativeCallbacks.AllocateMesh);
        }

        public static void CheckInitialized()
        {
            if (!get)            //load static scene with the instance scene
                UnityEngine.SceneManagement.SceneManager.LoadScene("StaticScene", LoadSceneMode.Additive);
        }

        public MeshFilter CreateMeshFilter()
        {
            return Instantiate(baseModel).GetComponent<MeshFilter>();
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

        // [DllImport(dllName, ExactSpelling = true)]
        // public static extern void ComputeColors(
            // [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[,] outColors, int outColorsSize,
            // [In][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[,] Vptr, int VSize,
            // int nV);
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

        // public static void AllocateMesh(int VSize, int FSize, [Out] IntPtr V, [Out] IntPtr F)
        // {
        //     var libiglMeshFilter = LibiglInterface.get.CreateMeshFilter();
        //     
        //     var mesh = new Mesh();
        //     mesh.name = "generated-mesh";
        //     libiglMeshFilter.mesh = mesh;
        //
        //     var VLayout = new[]
        //     {
        //         //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
        //         new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
        //         new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float16, 2, 1),
        //         new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.UNorm8, 4, 2)
        //     };
        //     
        //     var VCount = 8;
        //     mesh.SetVertexBufferParams(VCount, VLayout); //Note:sizeof one vertex is defined in the layout as 3*4B
        //     //So buffer size is vertices * 3 * 4B
        //
        //     var FCount = 12;
        //     mesh.SetIndexBufferParams(3 * FCount,
        //         IndexFormat.UInt32); //Note: Size of buffer is count * uint32 = 3 * faces*4B
        //
        //     var V = new NativeArray<float>(3 * VCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        //     var F = new NativeArray<uint>(3 * FCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        // }
        public static void AllocateMesh(float[,] v, int vlength, int[] f)
        {
            //temp
        }
    }
    
}