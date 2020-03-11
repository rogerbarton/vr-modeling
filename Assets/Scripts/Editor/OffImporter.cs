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
    /// <summary>
    /// Called whenever a .off file is imported by Unity
    /// Trigger this manually by right-click > Reimport in the project browser
    /// </summary>
    /// <param name="ctx">Used to store imported output objects</param>
    public override void OnImportAsset(AssetImportContext ctx)
    {
        //Note: any reference that is not the default for a component has to be added via ctx.AddObjectToAsset()

        #region Create Imported GameObject

        var gameObject = new GameObject();
        ctx.AddObjectToAsset("Main Object", gameObject);
        ctx.SetMainObject(gameObject);

        var mesh = new Mesh();
        mesh.name = "imported-mesh";
        ctx.AddObjectToAsset("Mesh", mesh);

        var meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        var meshRenderer = gameObject.AddComponent<MeshRenderer>();
        var material = new Material(Shader.Find("Diffuse"));
        meshRenderer.material = material;
        ctx.AddObjectToAsset("Material", material);

        #endregion

        #region Load Mesh with libigl
        
        //readOFF and get result as a NativeArray
        NativeArray<float> V;
        NativeArray<float> N = default; //may be empty
        NativeArray<uint> F;
        int VSize, NSize, FSize;
        unsafe
        {
            //Load OFF into Eigen Matrices and get the pointers here
            Native.LoadOFF(ctx.assetPath, out var VPtr, out VSize, out var NPtr, out NSize,
                out var FPtr, out FSize);

            //Convert the pointers to NativeArrays which we can create a mesh with
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
        }

        //Setup the buffers, then fill the data later
        var VertexBufferLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1)
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        };
        //Note:sizeof one vertex is defined in the layout as 3*4B
        //So buffer size is vertices * 3 * 4B
        mesh.SetVertexBufferParams(VSize, VertexBufferLayout);
        //Note: Size of buffer is count * uint32 = 3 * faces*4B
        mesh.SetIndexBufferParams(3 * FSize, IndexFormat.UInt32);

        //Fill the buffers with out data from libigl::readOFF
        mesh.SetVertexBufferData(V, 0, 0, 3 * VSize, 0);
        if (NSize > 0)
            mesh.SetVertexBufferData(N, 0, 0, 3 * VSize, 1);
        else
        {
            Debug.LogWarning("No normals provided, " +
                             "RecalculateNormals is currently not working with multiple vertex buffer streams. " +
                             "Use igl::per_vertex_normals() instead");
            mesh.RecalculateNormals();
        }

        mesh.SetIndexBufferData(F, 0, 0, 3 * FSize);

        //Create a submesh that will be rendered
        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * FSize));

        // mesh.RecalculateTangents();
        mesh.MarkModified();

        #endregion
    }
}
