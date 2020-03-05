using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Testing : MonoBehaviour
{
    // [StructLayout(LayoutKind.Sequential)]
    // struct VertexPos
    // {
    //     public float3 pos;
    // }
    //
    // [StructLayout(LayoutKind.Sequential)]
    // struct Face
    // {
    //     public int3 tri;
    // }
    
    void Start()
    {
        LibiglInterface.CheckInitialized();
        
        var mesh = new Mesh();
        mesh.name = "generated-mesh";
        GetComponent<MeshFilter>().mesh = mesh;
        
        var VLayout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
        };
        var VCount = 8;
        mesh.SetVertexBufferParams(VCount, VLayout);
        
        var FCount = 12;
        mesh.SetIndexBufferParams(3*FCount, IndexFormat.UInt32);
        
        var V = new NativeArray<float>(3 * VCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory); //Or Allocator.Persistent with Dispose()
        var F = new NativeArray<uint>(3 * FCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        
        //--- Call C++ to fill V, F
        unsafe
        {
            Native.FillMesh(V.GetUnsafePtr(), VCount, F.GetUnsafePtr(), FCount);
        }

        mesh.SetVertexBufferData(V, 0, 0, 3*VCount, 0, MeshUpdateFlags.DontValidateIndices);
        mesh.SetIndexBufferData(F, 0, 0, 3*FCount);
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3*FCount));
        
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        mesh.UploadMeshData(false);
        
        //OR use 
        // mesh.SetVertices(V);
        // mesh.SetIndices(F, MeshTopology.Triangles, 0);
        
        mesh.Optimize();

        // V.Dispose();
        // F.Dispose();
        // Debug.Log(mesh.GetSubMesh(0).topology);


        //--- To update an existing initialized mesh
        //Use this to get pointers to buffers, can pass IntPtr to a float* or int* in c++
        // IntPtr voidPtrToVArr = mesh.GetNativeVertexBufferPtr(0); //retrieve the first buffer
        // IntPtr voidPtrToFArr = mesh.GetNativeIndexBufferPtr();
    }

    private int value = 0;
    void Update()
    {
        if(Input.anyKeyDown)
            Debug.Log(value = Native.IncrementValue(value));
             
        // Native.LoadMesh("bumpy-cube.obj");
    }
}
