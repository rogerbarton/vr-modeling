using System;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

[ScriptedImporter(1, "off")]
public class OffImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {

        var gameObject = new GameObject();
        ctx.AddObjectToAsset("Main Object", gameObject);
        ctx.SetMainObject(gameObject);
        var meshFilter = gameObject.AddComponent<MeshFilter>();
        var mesh = meshFilter.mesh = new Mesh();
        mesh.name = "imported-mesh";

        //Get vertex and face count
        //readOFF and get result as a NativeArray
        NativeArray<float> V;
        NativeArray<float> N = default;
        NativeArray<uint> F;
        int VSize, NSize, FSize;
        unsafe
        {
            Native.LoadOFF(ctx.assetPath, out var VPtr, out VSize, out var NPtr, out NSize, out var FPtr,
                out FSize);
            //Note: length may be in bytes?
            V = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(VPtr, 3 * VSize, Allocator.Temp);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
            if (NSize > 0)
            {
                N = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(NPtr, 3 * VSize,
                    Allocator.Temp);
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
            }

            F = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<uint>(FPtr, 3 * FSize, Allocator.Temp);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());

            var VLayout = new[]
            {
                //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
            };
            //Setup the buffers
            //Note:sizeof one vertex is defined in the layout as 3*4B
            //So buffer size is vertices * 3 * 4B
            mesh.SetVertexBufferParams(VSize, VLayout);
            //Note: Size of buffer is count * uint32 = 3 * faces*4B
            mesh.SetIndexBufferParams(3 * FSize, IndexFormat.UInt32);

            //Fill the buffers with out data from libigl::readOFF
            mesh.SetVertexBufferData(V, 0, 0, 3 * VSize, 0);
            if (NSize > 0)
                mesh.SetVertexBufferData(N, 0, 0, 3 * VSize, 1);
            mesh.SetIndexBufferData(F, 0, 0, 3 * FSize);

            //Create a submesh that will be rendered
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * FSize));


            // mesh.RecalculateTangents();
            // mesh.UploadMeshData(true);

            mesh.MarkModified();
        }
    }
}
