using System;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class Testing : MonoBehaviour
{
    private MeshFilter meshFilter;
    Mesh mesh;
    private NativeArray<float> V;
    int VSize;
    bool uploadToGpu;    

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = meshFilter.mesh;
        VSize = mesh.vertexCount;
        V = new NativeArray<float>(3 * VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
        var layout = mesh.GetVertexAttributes();
        // mesh.MarkDynamic();
        unsafe
        {
            NativeArray<float> tmp;
            fixed(Vector3* managedVPtr = mesh.vertices)
                tmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>((float*)managedVPtr, 3 * VSize, Allocator.Temp);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref tmp, AtomicSafetyHandle.Create());
            // NativeArray<float>.Copy(tmp, V); //Make our own copy
            V.CopyFrom(tmp);
            
            tmp.Dispose();
        }
        mesh.UploadMeshData(true); //delete managed copy and make no longer readable via Unity
    }

    private void OnPostRender()
    {
        unsafe
        {
            if (uploadToGpu)
            {
                uploadToGpu = false;
                Native.UploadMesh((float*) mesh.GetNativeVertexBufferPtr(0).ToPointer(), (float*) V.GetUnsafePtr(),
                    VSize);
            }
        }

        // Use CommandBuffer.IssuePluginEventAndData
    }

    void Update()
    {
        if (Input.GetAxis("Horizontal") != 0f)
            TranslateMesh(new Vector3( Mathf.Sign(Input.GetAxis("Horizontal")), 0, 0));
        // TranslateMesh(new Vector3(Time.deltaTime * 1f, 0, 0));
    }

    private void TranslateMesh(Vector3 direction)
    {
        unsafe
        {
            Native.TranslateMesh((float*)V.GetUnsafePtr(), VSize, direction);
            uploadToGpu = true;
        }
        // mesh.MarkModified();
    }
    
    private void TranslateMeshManaged(Vector3 direction)
    {
        //TODO: causes crash in DirectX currently
        // var layout = mesh.GetNativeVertexBufferPtr();
        //TODO: Cannot modify directly, need to BeginModifyVertexBuffer and end in C++
        //Keep a copy as a NativeArray and SetVertexBuffer each time
        //Can only use this ptr to make a copy to a NativeArray on the CPU
        //OR use a map as in the link below in the BeginModifyVertexBuffer()
        //https://bitbucket.org/Unity-Technologies/graphicsdemos/pull-requests/2/example-of-native-vertex-buffers-for/diff
        
        unsafe 
        {
            fixed (Vector3* VPtr = mesh.vertices)
            {
                // var V = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(VPtr, 3 * VSize, Allocator.Temp);
                Native.TranslateMesh((float*) VPtr, VSize, direction);
            }
        }

        mesh.SetVertices(V);
        
        
        //Set vertexbufferdata?
        // mesh.MarkModified();
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
        
        // V.Dispose();
        // F.Dispose();
    }
    
    private void OnDestroy()
    {
        if(V.IsCreated)
            V.Dispose();
    }
}
