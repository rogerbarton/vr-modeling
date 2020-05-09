using System;
using libigl.Behaviour;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace libigl
{
    /// <summary>
    /// Stores a copy of the Unity Mesh's arrays. This is purely for the interface between the Libigl Mesh State and
    /// the Unity Mesh / what will be rendered.
    /// Uses RowMajor.
    /// </summary>
    public class UMeshData : IDisposable
    {
        public const bool IsRowMajor = true;

        /// <summary>
        /// Marks which data has changed in <c>MeshDataNative</c> as a bitmask
        /// </summary>
        public class DirtyFlag
        {
            public const uint None = 0;
            public const uint All = uint.MaxValue - 32 - 64;
            public const uint VDirty = 1;
            public const uint NDirty = 2;
            public const uint CDirty = 4;
            public const uint UVDirty = 8;
            public const uint FDirty = 16;
            /// <summary>
            /// Don't recaluclate normals when VDirty is set, <see cref="NDirty"/> overrides this.
            /// </summary>
            public const uint DontComputeNormals = 32;
            /// <summary>
            /// Don't recalculate bounds when VDirty is set. Bounds are used for occlusion culling.
            /// </summary>
            public const uint DontComputeBounds = 64;
        }

        public uint DirtyState = DirtyFlag.None;
        
        public NativeArray<Vector3> V;
        public NativeArray<Vector3> N;
        public NativeArray<Color> C;
        public NativeArray<Vector2> UV;
        public NativeArray<int> F;
        public readonly int VSize;
        public readonly int FSize;

        private UMeshDataNative _native;

        /// <summary>
        /// Create a copy of the mesh arrays
        /// </summary>
        /// <param name="mesh">Unity Mesh to copy from</param>
        /// <param name="isRowMajor">Are the underlying arrays in RowMajor</param>
        /// <param name="onlyAllocateMemory">Should the arrays be uninitialized.
        /// If true memory is only allocated but no data is copied from the mesh.</param>
        public UMeshData(Mesh mesh)
        {
            VSize = mesh.vertexCount;
            FSize = mesh.triangles.Length / 3;

            // Allocate & Copy the V, F matrices from the mesh
            Allocate(mesh);
            CopyFrom(mesh);
        }

        /// <summary>
        /// Allocated the NativeArrays once VSize and FSize have been set.
        /// </summary>
        private void Allocate(Mesh mesh)
        {
            Assert.IsTrue(VSize > 0 && FSize > 0);
            Assert.IsTrue(!V.IsCreated);
            
            // TODO: Allocate these in C++ as Eigen Matrices so we can avoid using maps,
            //       then use ConvertExistingDataToNativeArray to get a C# representation for the Unity Mesh
            //       CopyFrom: pass normal Vector3[] to C++, will be a 1D LPArray, should work
            //       Use 'new' in C++ as with the state
            //       Can be disposed by C# or C++ in DisposeMesh
            //       
            //       Will be fixed by default and we can store the pointers in the State
            //       Can have only one state in C++, only need to return/set DirtyFlags for C#
            V = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            N = new NativeArray<Vector3>(VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            C = new NativeArray<Color>(VSize, Allocator.Persistent, mesh.colors.Length == 0 ? NativeArrayOptions.UninitializedMemory : NativeArrayOptions.ClearMemory);
            UV = new NativeArray<Vector2>(VSize, Allocator.Persistent, mesh.uv.Length == 0 ? NativeArrayOptions.UninitializedMemory : NativeArrayOptions.ClearMemory);
            F = new NativeArray<int>(3 * FSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Before we can use this we need to add a safety handle (only in the editor)
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref N, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref C, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref UV, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());
#endif
            
            // Store the native pointers int _native
            unsafe
            {
                _native = new UMeshDataNative((float*) V.GetUnsafePtr(), (float*) N.GetUnsafePtr(),
                    (float*) C.GetUnsafePtr(), (float*) UV.GetUnsafePtr(), (int*) F.GetUnsafePtr(), VSize, FSize);
            }
        }
        
        /// <summary>
        /// Copies all data (e.g. V, F) from Unity mesh into the <b>allocated</b> NativeArrays
        /// </summary>
        private void CopyFrom(Mesh mesh)
        {
            Assert.IsTrue(V.IsCreated);
            
            V.CopyFrom(mesh.vertices);
            N.CopyFrom(mesh.normals);
            if(mesh.colors.Length > 0)
                C.CopyFrom(mesh.colors);
            if(mesh.uv.Length > 0)
                UV.CopyFrom(mesh.uv);
            F.CopyFrom(mesh.triangles);
        }

        /// <summary>
        /// Applies changes to another copy of the data with a different IsRowMajor
        /// Use this to copy changes from Col to RowMajor, vice versa
        /// Can be called from a worker thread
        /// <param name="from">Where to apply changes to. </param>
        /// <param name="propagateDirtyState">Should <see cref="to"/> have the same dirtyState, or set it to <see cref="DirtyFlag.None"/> if false</param>
        /// </summary>
        public unsafe void ApplyDirty(State* state)
        {
            Assert.IsTrue(VSize == state->VSize && FSize == state->FSize);
            
            if(state->DirtyState == DirtyFlag.None) return;

            // Copy over and transpose data that has changed
            Native.ApplyDirty(state, _native);
            
            DirtyState |= state->DirtyState;
            state->DirtyState = DirtyFlag.None;
        }

        /// <summary>
        /// Apply MeshData changes to the Unity Mesh to see changes when rendered.
        /// Must be called on the main thread with RowMajor data.
        /// </summary>
        public void ApplyDirtyToMesh(Mesh mesh)
        {
            if(DirtyState == DirtyFlag.None) return;
            Assert.IsTrue(IsRowMajor, "Data must be in RowMajor format to apply changes to the Unity mesh.");

            if ((DirtyState & DirtyFlag.VDirty) > 0)
            {
                mesh.SetVertices(V);
                if ((DirtyState & DirtyFlag.DontComputeBounds) == 0)
                    mesh.RecalculateBounds();
                if ((DirtyState & DirtyFlag.DontComputeNormals & DirtyState & DirtyFlag.NDirty) == 0)
                    mesh.RecalculateNormals();
            }

            if ((DirtyState & DirtyFlag.NDirty) > 0)
                mesh.SetNormals(N);
            if ((DirtyState & DirtyFlag.CDirty) > 0)
                mesh.SetColors(C);
            if ((DirtyState & DirtyFlag.UVDirty) > 0)
                mesh.SetUVs(0, UV);
            if ((DirtyState & DirtyFlag.FDirty) > 0)
                mesh.SetIndices(F, MeshTopology.Triangles, 0);

            DirtyState = DirtyFlag.None;
        }
        
        /// <summary> 
        /// Note: Changes to the dirtyState are not applied to the MeshData instance immediately (not a reference) and 
        /// needs to be set manually one the native function has been called. 
        /// </summary> 
        /// <returns>A MeshDataNative instance than can be passed to C++ containing all pointers</returns> 
        public UMeshDataNative GetNative()
        {
            return _native;
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