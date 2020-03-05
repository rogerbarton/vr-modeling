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
        GetComponent<MeshFilter>().mesh = mesh;
        
        var VLayout = new[]
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3)
        };
        var VCount = 8;
        mesh.SetVertexBufferParams(VCount, VLayout);
        
        var FCount = 12;
        mesh.SetIndexBufferParams(FCount, IndexFormat.UInt32);
        
        var V = new NativeArray<float3>(VCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory); //Or Allocator.Persistent with Dispose()
        var F = new NativeArray<int3>(FCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        
        //--- Call C++ to fill V, F
        unsafe
        {
            Native.FillMesh(V.GetUnsafePtr(), VCount, F.GetUnsafePtr(), FCount);
        }

        mesh.SetVertexBufferData(V, 0, 0, VCount, 0, MeshUpdateFlags.DontValidateIndices);
        mesh.SetIndexBufferData(F, 0, 0, FCount, MeshUpdateFlags.DontValidateIndices);
        
        // V.Dispose();
        // F.Dispose();
        
        
        
        //--- To update an existing initialized mesh
        //Use this to get pointers to buffers, can pass IntPtr to a float* or int* in c++
        IntPtr voidPtrToVArr = mesh.GetNativeVertexBufferPtr(0); //retrieve the first buffer
        IntPtr voidPtrToFArr = mesh.GetNativeIndexBufferPtr();
        
    }

    private int value = 0;
    void Update()
    {
        if(Input.anyKeyDown)
            Debug.Log(value = Native.IncrementValue(value));
             
        // Native.LoadMesh("bumpy-cube.obj");
    }
}
