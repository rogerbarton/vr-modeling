using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace libigl
{
    /// <summary>
    /// Contains all callbacks from the native context
    /// C++ to C# Communication
    /// </summary>
    public static class NativeCallbacks
    {
        //Based on https://answers.unity.com/questions/30620/how-to-debug-c-dll-code.html
        // The 'function pointer' passed to C++
        public delegate void StringCallback(string message);

        //The function that we point to in C++
        public static void DebugLog(string message)
        {
            Debug.Log("[c++] " + message);
        }
        
        public static void DebugLogWarning(string message)
        {
            Debug.LogWarning("[c++] " + message);
        }
        
        public static void DebugLogError(string message)
        {
            Debug.LogError("[c++] " + message);
        }

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
    }
}