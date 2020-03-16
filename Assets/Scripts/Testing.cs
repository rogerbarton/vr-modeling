using System;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

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

    private MeshFilter meshFilter;
    
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        // NativeEditor.Initialize();
        // Native.Initialize();
        // Mesh mesh = meshFilter.mesh;
        // mesh.MarkDynamic();
        //
        // var VLayout = new[]
        // {
        //     //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
        //     new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
        //     new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
        //     new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
        //     new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
        //     new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        // };
        // var oldLayout = mesh.GetVertexAttributes();
        // mesh.SetVertexBufferParams(mesh.vertexCount, VLayout);
        // mesh.RecalculateTangents();
        
        // CreateCube();
    }

    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0f)
            TranslateMesh(new Vector3(Time.deltaTime * Input.GetAxis("Horizontal"), 0, 0));
        // TranslateMesh(new Vector3(Time.deltaTime * 1f, 0, 0));
    }

    private void TranslateMesh(Vector3 direction)
    {
        //TODO: causes crash in DirectX currently
        var mesh = meshFilter.mesh;
        mesh.MarkDynamic();
        var layout = mesh.GetVertexAttributes();
        //TODO: Cannot modify directly, need to BeginModifyVertexBuffer and end in C++
        //Keep a copy as a NativeArray and SetVertexBuffer each time
        //Can only use this ptr to make a copy to a NativeArray on the CPU
        //OR use a map as in the link below in the BeginModifyVertexBuffer()
        //https://bitbucket.org/Unity-Technologies/graphicsdemos/pull-requests/2/example-of-native-vertex-buffers-for/diff
        
        var VSize = mesh.vertexCount;
        var V = mesh.vertices;
        unsafe 
        {
            fixed (Vector3* VPtr = V)
            {
                // var V = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(VPtr, 3 * VSize, Allocator.Temp);
                Native.TranslateMesh((float*) VPtr, VSize, direction);
            }
        }

        mesh.vertices = V;
        
        
        //Set vertexbufferdata?
        mesh.MarkModified();
        mesh.UploadMeshData(false);
    }


    /// <summary>
    /// Test function that creates a new mesh with a cube
    /// </summary>
    private void CreateCube()
    {
        var mesh = new Mesh();
        mesh.name = "generated-mesh";
        meshFilter.mesh = mesh;
        //mesh.MarkDynamic(); //if we want to repeatedly modify the mesh

        var VLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
            new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        };
        var VCount = 8;
        mesh.SetVertexBufferParams(VCount, VLayout); //Note:sizeof one vertex is defined in the layout as 3*4B
        //So buffer size is vertices * 3 * 4B
        
        var FCount = 12;
        mesh.SetIndexBufferParams(3 * FCount, IndexFormat.UInt32); //Note: Size of buffer is count * uint32 = 3 * faces*4B
        
        var V = new NativeArray<float>(3 * VCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        var F = new NativeArray<uint>(3 * FCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        //Or Allocator.Persistent with Dispose()
        
        //--- Call C++ to fill V, F
        unsafe
        {
            // Native.FillMesh(V.GetUnsafePtr(), VCount, F.GetUnsafePtr(), FCount);
        }

        //Be sure to multiply by 3 to copy whole array
        mesh.SetVertexBufferData(V, 0, 0, 3*VCount, 0);
        mesh.SetIndexBufferData(F, 0, 0, 3*FCount);
        //Note! Set*BufferData incurs a memcpy
        
        //Create one submesh which will be rendered (required)
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3*FCount));
        
        //Not sure if this is needed
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.UploadMeshData(true);//for optimization, cannot edit/view in debugger after issuing this
        
        mesh.MarkModified(); //As we DontNotifyMeshUsers

        
        
        //--- To update an existing initialized mesh
        //Use this to get pointers to buffers, can pass IntPtr to a float* or int* in c++
        // IntPtr voidPtrToVArr = mesh.GetNativeVertexBufferPtr(0); //retrieve the first buffer
        // IntPtr voidPtrToFArr = mesh.GetNativeIndexBufferPtr();
        
        V.Dispose();
        F.Dispose();
    }
}
