using System;
using libigl;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace libigl
{
    [ScriptedImporter(1, "off")]
    public class OffImporter : ScriptedImporter
    {
        public float scale = 1f;
        [Tooltip("Reorders vertices and faces for better rendering performance.")]
        public bool optimizeForRendering;

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
                Native.LoadOFF(ctx.assetPath, scale, out var VPtr, out VSize, out var NPtr, out NSize,
                    out var FPtr, out FSize);

                //Convert the pointers to NativeArrays which we can create a mesh with
                V = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(VPtr, 3 * VSize, Allocator.Temp);

                if (NSize > 0)
                {
                    N = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(NPtr, 3 * VSize,
                        Allocator.Temp);
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
                    #endif
                }

                F = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<uint>(FPtr, 3 * FSize, Allocator.Temp);
                #if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());
                #endif
            }

            //Setup the buffers, then fill the data later
            
            //Note:sizeof one vertex is defined in the layout as 3*4B
            //So buffer size is vertices * 3 * 4B
            mesh.SetVertexBufferParams(VSize, Native.VertexBufferLayout);
            //Note: Size of buffer is count * uint32 = 3 * faces*4B
            mesh.SetIndexBufferParams(3 * FSize, IndexFormat.UInt32);

            //Fill the buffers with out data from libigl::readOFF
            mesh.SetVertexBufferData(V, 0, 0, 3 * VSize, 0);
            mesh.SetIndexBufferData(F, 0, 0, 3 * FSize);

            //Create a submesh that will be rendered
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * FSize));

            if (NSize > 0)
                mesh.SetVertexBufferData(N, 0, 0, 3 * VSize, 1);
            else
            {
                // Alternatively use igl::per_vertex_normals() instead
                mesh.RecalculateNormals();
            }
            
            if(optimizeForRendering)
                mesh.Optimize();
            
            // mesh.RecalculateTangents();
            mesh.MarkDynamic(); //keep a copy on the cpu side and make gpu buffers cpu writable
            mesh.MarkModified();
            mesh.RecalculateBounds();
            #endregion
        }
    }
}
