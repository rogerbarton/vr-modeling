using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace Libigl
{
    /// <summary>
    /// Marks which data has changed in <c>UMeshData</c> as a bitmask and needs to be applied to the mesh.<br/>
    /// This is used to selectively update the Unity mesh and is only for data that Unity requires.
    /// Use these constants along with the bitwise operators.
    /// </summary>
    public class DirtyFlag
    {
        public const uint None = 0;
        public const uint VDirty = 1;
        public const uint NDirty = 2;
        public const uint CDirty = 4;
        public const uint UVDirty = 8;
        public const uint FDirty = 16;

        /// <summary>
        /// Don't recaluclate normals when <see cref="VDirty"/> is set, <see cref="NDirty"/> overrides this.
        /// </summary>
        public const uint DontComputeNormals = 32;

        /// <summary>
        /// Don't recalculate bounds when <see cref="VDirty"/> is set. Bounds are used for occlusion culling.
        /// </summary>
        public const uint DontComputeBounds = 64;

        /// <summary>
        /// Don't recompute colors if a visible selection has changed.
        /// </summary>
        public const uint DontComputeColorsBySelection = 128;

        /// <summary>
        /// Use this when the vertex positions have changed, but the boundary conditions are unaffected.
        /// <see cref="VDirty"/> overrides this.
        /// </summary>
        public const uint VDirtyExclBoundary = 256;
        public const uint All = uint.MaxValue - DontComputeNormals - DontComputeBounds;
    }

    /// <summary>
    /// Stores a copy of the Unity Mesh's arrays. This is purely for the interface between the Libigl Mesh <see cref="State"/>
    /// and the Unity Mesh / what will be rendered.
    /// Important: Uses RowMajor as that is how it is stored by Unity and on the GPU.
    /// </summary>
    public class UMeshData : IDisposable
    {
        public const bool IsRowMajor = true;

        // Which data has changed and needs to be applied to the mesh
        public uint DirtyState = DirtyFlag.None;
        public uint DirtySelections = 0;
        public uint DirtySelectionsResized = 0;
        
        public NativeArray<Vector3> V;
        public NativeArray<Vector3> N;
        public NativeArray<Color> C;
        public NativeArray<Vector2> UV;
        public NativeArray<int> F;
        public NativeArray<uint> S; // VectorXi, Points to C++ data in the State
        public readonly int VSize;
        public readonly int FSize;

        /// <summary>
        /// Stores pointers to the native arrays, we can pass this to C++
        /// </summary>
        private UMeshDataNative _native;

        /// <param name="mesh">Unity Mesh to copy from</param>
        public UMeshData(Mesh mesh)
        {
            VSize = mesh.vertexCount;
            FSize = mesh.triangles.Length / 3;

            // Allocate & Copy the V, F matrices from the mesh
            Allocate(mesh);
            CopyFrom(mesh);
        }
        
        /// <summary>
        /// Initialize the UMeshData with shared data with the <paramref name="behaviour"/>.
        /// </summary>
        public unsafe void LinkBehaviourState(LibiglBehaviour behaviour)
        {
            S = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<uint>(behaviour.State->S, VSize, Allocator.None);
            
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref S, AtomicSafetyHandle.Create());
#endif
        }

        /// <summary>
        /// Allocated the NativeArrays once VSize and FSize have been set.
        /// </summary>
        private void Allocate(Mesh mesh)
        {
            Assert.IsTrue(VSize > 0 && FSize > 0);
            Assert.IsTrue(!V.IsCreated);
            
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
                // NativeArrays will be fixed by default so we can get these pointers only once, not every time we use them
                _native = new UMeshDataNative((float*) V.GetUnsafePtr(), (float*) N.GetUnsafePtr(),
                    (float*) C.GetUnsafePtr(), (float*) UV.GetUnsafePtr(), (int*) F.GetUnsafePtr(), VSize, FSize);
            }
        }
        
        /// <summary>
        /// Copies all data (e.g. V, F) from Unity mesh into the <b>already allocated</b> NativeArrays
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
        /// Applies changes to the C++ State to this instance. Use this to copy changes from Col to RowMajor.<br/>
        /// Can and should be called from a worker thread. Behind the scenes this tranposes and copies the matrices.<br/>
        /// The DirtyState is propagated so <see cref="ApplyDirtyToMesh"/> (called on the main thread) will apply the changes.
        /// <seealso cref="Native.ApplyDirty"/>
        /// </summary>
        public unsafe void ApplyDirty(State* state, InputState inputState)
        {
            Assert.IsTrue(VSize == state->VSize && FSize == state->FSize);
            
            // Copy over and transpose data that has changed
            Native.ApplyDirty(state, _native, inputState.VisibleSelectionMask);
            
            DirtyState |= state->DirtyState;
            DirtySelections |= state->DirtySelections;
            DirtySelectionsResized |= state->DirtySelectionsResized;
        }

        /// <summary>
        /// Apply MeshData changes to the Unity Mesh to see changes when rendered.
        /// Uses the <see cref="DirtyState"/> to detect what needs to be applied.<br/>
        /// Must be called on the main thread as it accesses the Unity API.
        /// <remarks>Assert: <see cref="IsRowMajor"/> is true.</remarks>
        /// </summary>
        public void ApplyDirtyToMesh(Mesh mesh)
        {
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
            // if (DirtySelections > 0)
                // mesh.SetUVs(1, S);
            // BUG: mesh.SetUVs is from the older API and expects a Vector2[] (floats)
            // Unsupported conversion of vertex data (format 11 to 0, dimensions 1 to 1)
            // UnityEngine.Mesh:SetUVs(Int32, NativeArray`1)
            
            DirtyState = DirtyFlag.None;
            DirtySelections = 0;
            DirtySelectionsResized = 0;
        }
        
        /// <summary> 
        /// Note: Changes to the dirtyState are not applied to the MeshData instance (not a reference) and 
        /// needs to be set manually in a C# context.<br/>
        /// Important: The struct itself should be treated as <c>const</c> as changes have no effect (it's a copy).
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
            // S disposed by C++
        }
    }
}