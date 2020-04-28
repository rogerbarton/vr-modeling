using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace libigl
{
    /// <summary>
    /// Stores all native arrays for a mesh
    /// </summary>
    public class MeshData : IDisposable
    {
        public readonly bool IsRowMajor;

        [Flags]
        public enum DirtyFlag
        {
            None = 0,
            VDirty = 1,
            NDirty = 2,
            CDirty = 4,
            UVDirty = 8,
            FDirty = 16,
            /// <summary>
            /// Recalculate normals, <see cref="NDirty"/> overrides this.
            /// </summary>
            ComputeNormals = 32
        }    

        public DirtyFlag DirtyState = DirtyFlag.None;
        
        public NativeArray<Vector3> V;
        public NativeArray<Vector3> N;
        public NativeArray<Color> C;
        public NativeArray<Vector2> UV;
        public NativeArray<int> F;
        public readonly int VSize;
        public readonly int FSize;

        /// <summary>
        /// Create a copy of the mesh arrays
        /// </summary>
        /// <param name="mesh">Unity Mesh to copy from</param>
        /// <param name="isRowMajor">Are the underlying arrays in RowMajor</param>
        /// <param name="onlyAllocateMemory">Should the arrays be uninitialized.
        /// If true memory is only allocated but no data is copied from the mesh.</param>
        public MeshData(Mesh mesh, bool isRowMajor = false, bool onlyAllocateMemory = false)
        {
            IsRowMajor = isRowMajor;
            
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

            if (!onlyAllocateMemory)
            {
                //Copy the V, F matrices from the mesh
                V.CopyFrom(mesh.vertices);
                N.CopyFrom(mesh.normals);
                C.CopyFrom(mesh.colors);
                UV.CopyFrom(mesh.uv);
                F.CopyFrom(mesh.triangles);

                if (!isRowMajor)
                    TransposeInPlace();
            }
        }
        
        public void TransposeInPlace()
        {
            V.TransposeInPlace();
            N.TransposeInPlace();
            C.TransposeInPlace(0, 4);
            UV.TransposeInPlace(0, 2);
            F.TransposeInPlace(F.Length / 3);
        }
        
        
        /// <summary>
        /// Applies changes to another copy of the data with a different IsRowMajor
        /// Use this to copy changes from Col to RowMajor, vice versa
        /// <param name="to">Where to apply changes to. </param>
        /// <param name="propagateDirtyState">Should <see cref="to"/> have the same dirtyState, or set it to <see cref="DirtyFlag.None"/> if false</param>
        /// </summary>
        public void ApplyDirtyToTranspose(MeshData to, bool propagateDirtyState = true)
        {
            if(DirtyState == DirtyFlag.None) return;
            
            Assert.IsTrue((DirtyState & to.DirtyState) == DirtyFlag.None, 
                "Warning! 'to' MeshData has dirty changes that will be overwritten. " +
                "You need to call MeshData.ApplyDirtyToTranspose before making changes to the copy of MeshData.");
            
            Assert.IsTrue(IsRowMajor != to.IsRowMajor, "Both MeshData instances are of the same type Col/RowMajor.");
            
            if((DirtyState & DirtyFlag.VDirty) > 0)
                V.TransposeTo(to.V);
            if((DirtyState & DirtyFlag.NDirty) > 0)
                N.TransposeTo(to.N);
            if((DirtyState & DirtyFlag.CDirty) > 0)
                C.TransposeTo(to.C, 0, 4);
            if((DirtyState & DirtyFlag.UVDirty) > 0)
                UV.TransposeTo(to.UV, 0, 2);
            if((DirtyState & DirtyFlag.FDirty) > 0)
                F.TransposeTo(to.F, F.Length / 3);

            if(propagateDirtyState)
                to.DirtyState |= DirtyState;
            DirtyState = DirtyFlag.None;
        }

        /// <summary>
        /// Apply MeshData changes to the Unity Mesh to see changes when rendered.
        /// Must be called on the main thread with RowMajor data.
        /// </summary>
        public void ApplyChangesToMesh(Mesh mesh)
        {
            if(DirtyState == DirtyFlag.None) return;
            Assert.IsTrue(IsRowMajor, "Data must be in RowMajor format to apply changes to the Unity mesh.");
            
            if ((DirtyState & DirtyFlag.VDirty) > 0)
                mesh.SetVertices(V);
            if ((DirtyState & DirtyFlag.NDirty) > 0)
                mesh.SetNormals(N);
            else if ((DirtyState & DirtyFlag.None) > 0)
                mesh.RecalculateNormals();
            if ((DirtyState & DirtyFlag.CDirty) > 0)
                mesh.SetColors(C);
            if ((DirtyState & DirtyFlag.UVDirty) > 0)
                mesh.SetUVs(0, UV);
            if ((DirtyState & DirtyFlag.FDirty) > 0)
                mesh.SetIndices(F, MeshTopology.Triangles, 0);

            DirtyState = DirtyFlag.None;
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