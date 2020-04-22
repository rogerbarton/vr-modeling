using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl
{
    /// <summary>
    /// Stores all native arrays for a mesh
    /// </summary>
    public class MeshData : IDisposable
    {
        public NativeArray<Vector3> V;
        public NativeArray<Vector3> N;
        public NativeArray<Color> C;
        public NativeArray<Vector2> UV;
        public NativeArray<int> F;
        public int VSize;
        public int FSize;

        /// <summary>
        /// Create a copy of the mesh arrays
        /// </summary>
        /// <param name="mesh">Unity Mesh to copy from</param>
        public MeshData(Mesh mesh)
        {
            // Create a vertex data copy as a NativeArray<float> which we will pass to libigl
            VSize = mesh.vertexCount;
            FSize = mesh.triangles.Length;

            V = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            N = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            C = new NativeArray<Color>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            UV = new NativeArray<Vector2>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            F = new NativeArray<int>(FSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Before we can use this we need to add a safety handle (only in the editor)
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref C, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref UV, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());
#endif

            //Copy the V, F matrices from the mesh
            V.CopyFrom(mesh.vertices);
            N.CopyFrom(mesh.normals);
            C.CopyFrom(mesh.colors);
            UV.CopyFrom(mesh.uv);
            F.CopyFrom(mesh.triangles);
        }

        public void Dispose()
        {
            if (V.IsCreated) V.Dispose();
            if (N.IsCreated) N.Dispose();
            if (C.IsCreated) C.Dispose();
            if (UV.IsCreated) UV.Dispose();
            if (F.IsCreated) F.Dispose();
        }
    }
}