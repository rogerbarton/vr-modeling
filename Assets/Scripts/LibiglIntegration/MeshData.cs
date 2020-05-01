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
        public enum DirtyFlag : uint
        {
            None = 0,
            All = uint.MaxValue,
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
        public MeshData(Mesh mesh, bool isRowMajor = true, bool onlyAllocateMemory = false)
        {
            IsRowMajor = isRowMajor;
            
            VSize = mesh.vertexCount;
            FSize = mesh.triangles.Length;
            
            Allocate(mesh.colors.Length == 0, mesh.uv.Length == 0);
            
            if (!onlyAllocateMemory)
            {
                //Copy the V, F matrices from the mesh
                V.CopyFrom(mesh.vertices);
                N.CopyFrom(mesh.normals);
                if(mesh.colors.Length > 0) // TODO: What if the mesh has no colors, ie mesh.colors.Length == 0
                    C.CopyFrom(mesh.colors);
                if(mesh.uv.Length > 0)
                    UV.CopyFrom(mesh.uv);
                F.CopyFrom(mesh.triangles);

                if (!isRowMajor) // Will crash currently
                    TransposeInPlace(); 
            }
        }

        /// <summary>
        /// Construct a MeshData instance from another which has the opposite isRowMajor.
        /// Use case: constructing ColMajor meshData from existing RowMajor data, avoiding a TransposeInPlace.
        /// <code>Assert: IsRowMajor != other.IsRowMajor</code>
        /// </summary>
        /// <param name="other">Existing initialized MeshData with opposite isRowMajor.</param>
        /// <param name="isRowMajor">Are the underlying arrays in RowMajor</param>
        public MeshData(MeshData other, bool isRowMajor = false)
        {
            Assert.IsTrue(IsRowMajor != other.IsRowMajor, "Both MeshData instances are of the same type Col/RowMajor.");
            
            IsRowMajor = isRowMajor;
            VSize = other.VSize;
            FSize = other.FSize;
            
            Allocate();

            // Copy over data without a TransposeInPlace
            other.V.TransposeTo(V, other.IsRowMajor);
            other.N.TransposeTo(N, other.IsRowMajor);
            other.C.TransposeTo(C, other.IsRowMajor, 0, 4); 
            other.UV.TransposeTo(UV, other.IsRowMajor, 0, 2);
            other.F.TransposeTo(F, other.IsRowMajor, F.Length / 3);
        }

        /// <summary>
        /// Allocated the NativeArrays once VSize and FSize have been set.
        /// </summary>
        private void Allocate(bool hasColor = true, bool hasUv = true) //TODO: find a better way of allocating color/uv
        {
            Assert.IsTrue(VSize > 0 && FSize > 0);
            Assert.IsTrue(!V.IsCreated);
            
            V = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            N = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            C = new NativeArray<Color>(VSize, Allocator.Persistent, hasColor ? NativeArrayOptions.UninitializedMemory : NativeArrayOptions.ClearMemory);
            UV = new NativeArray<Vector2>(VSize, Allocator.Persistent, hasUv ? NativeArrayOptions.UninitializedMemory : NativeArrayOptions.ClearMemory);
            F = new NativeArray<int>(FSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Before we can use this we need to add a safety handle (only in the editor)
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref C, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref UV, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());
#endif
        }

        /// <summary>
        /// Transpose all arrays in place to convert between ColMajor and RowMajor.<br/>
        /// [Experimental] Due to Eigen::Map not allowing transposeInPlace 
        /// </summary>
        public void TransposeInPlace()
        {
            throw new NotImplementedException();
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
            
            Assert.IsTrue(VSize == to.VSize && FSize == to.FSize);
            Assert.IsTrue(IsRowMajor != to.IsRowMajor, "Both MeshData instances are of the same type Col/RowMajor.");
            Assert.IsTrue(to != this);
            
            if((DirtyState & DirtyFlag.VDirty) > 0)
                V.TransposeTo(to.V, IsRowMajor);
            if((DirtyState & DirtyFlag.NDirty) > 0)
                N.TransposeTo(to.N, IsRowMajor);
            if((DirtyState & DirtyFlag.CDirty) > 0)
                C.TransposeTo(to.C, IsRowMajor, 0, 4);
            if((DirtyState & DirtyFlag.UVDirty) > 0)
                UV.TransposeTo(to.UV, IsRowMajor, 0, 2);
            if((DirtyState & DirtyFlag.FDirty) > 0)
                F.TransposeTo(to.F, IsRowMajor, F.Length / 3);

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

        /// <returns>A MeshDataNative instance than can be passed to C++ containing all pointers</returns>
        public unsafe MeshDataNative GetNative()
        {
            return new MeshDataNative((float*) V.GetUnsafePtr(), (float*) N.GetUnsafePtr(), 
                (float*) C.GetUnsafePtr(), (float*) UV.GetUnsafePtr(), (int*) F.GetUnsafePtr(), VSize, FSize);
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