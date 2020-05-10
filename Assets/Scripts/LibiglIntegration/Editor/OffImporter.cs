using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Rendering;

namespace libigl.Editor
{
    [ScriptedImporter(1, "off")]
    public class OffImporter : ScriptedImporter
    {
        [Tooltip("Make the center/origin of the mesh the mean vertex. y center will be center of bounding box.")]
        public bool centerToMean = true;
        [Tooltip("Normalize the scale so that the y-height will be <scale>")]
        public bool normalizeScale = true;
        [Tooltip("Desired scale, if normalize is true this will be the y-height, else this will be a scaling factor on the mesh input.")]
        public float scale = 1f;
        
        [Tooltip("Reorders vertices and faces for better rendering performance.")]
        public bool optimizeForRendering = true;
        
        [Tooltip("Optional, default material is set by DefaultMaterialName in the script.")]
        public Material material;
        private static Material _defaultMaterial;
        // Use this material if a model specific material is not set (must be located in Models or Materials folder)
        private const string DefaultMaterialName = "OffDefault";
        // Use this shader if we cant find the default material by name
        private const string _DefaultMaterialNameFallbackShader = "Universal Render Pipeline/Lit";

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
            var newMaterial = material;
            if(!newMaterial) // Set material as default if it is null
            {
                if(!_defaultMaterial)
                {
                    var guids = AssetDatabase.FindAssets($"{DefaultMaterialName} t:{nameof(material)}", 
                        new[] {"Assets/Models", "Assets/Materials"});
                    if(guids.Length > 0)
                    {
                        _defaultMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guids[0]));
                    }
                    else
                    {
                        _defaultMaterial = new Material(Shader.Find(_DefaultMaterialNameFallbackShader));
                        Debug.LogWarning($"Could not find material asset with DefaultMaterialName: {DefaultMaterialName}, using fallback shader.");
                    }
                }

                newMaterial = _defaultMaterial;
            }
            meshRenderer.material = newMaterial;
            // Copy the material into the imported mesh if it is not the default by name
            if(newMaterial.name != DefaultMaterialName)
                ctx.AddObjectToAsset("Material", newMaterial);

            #endregion

            #region Load Mesh with libigl

            //readOFF and get result as a NativeArray
            NativeArray<Vector3> V;
            NativeArray<Vector3> N = default; //may be empty
            NativeArray<int> F;
            int VSize, NSize, FSize;
            unsafe
            {
                //Load OFF into Eigen Matrices and get the pointers here
                Native.LoadOFF(ctx.assetPath, centerToMean, normalizeScale, scale, out var VPtr, out VSize, out var NPtr, out NSize,
                    out var FPtr, out FSize);

                //Convert the pointers to NativeArrays which we can create a mesh with
                V = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(VPtr, VSize, Allocator.Temp);

                if (NSize > 0)
                {
                    N = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(NPtr, VSize,
                        Allocator.Temp);
                    #if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
                    #endif
                }

                F = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(FPtr, 3 * FSize, Allocator.Temp);
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
            mesh.SetVertices(V);
            mesh.SetIndices(F, MeshTopology.Triangles, 0);

            //Create a submesh that will be rendered
            // mesh.subMeshCount = 1;
            // mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * FSize));

            if (NSize > 0)
                mesh.SetNormals(N);
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
