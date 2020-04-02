using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace libigl
{
    /// <summary>
    /// An interface for modifying a mesh with libigl
    /// The deformation executed is defined in the LibiglJob.Execute()
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LibiglMesh : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private Mesh mesh;
        private NativeArray<float> V;
        private NativeArray<int> F;
        private int VSize;
        private int FSize;

        Stopwatch timer = new Stopwatch();
        JobHandle? libiglJobHandle;

        void Start()
        {
            // Get a reference to the mesh 
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
            mesh.MarkDynamic(); //Allow us to modify it at runtime

            // Create a vertex data copy as a NativeArray<float> which we will pass to libigl
            VSize = mesh.vertexCount;
            FSize = mesh.triangles.Length / 3;

            V = new NativeArray<float>(3 * VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            F = new NativeArray<int>(3 * FSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Before we can use this we need to add a safety handle (only in the editor)
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref F, AtomicSafetyHandle.Create());
#endif

            //Copy the V, F matrices from the mesh
            unsafe
            {
                NativeArray<float> VTmp;
                fixed (Vector3* managedVPtr = mesh.vertices)
                    VTmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                        (float*) managedVPtr, 3 * VSize, Allocator.Temp);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref VTmp, AtomicSafetyHandle.Create());
#endif

                V.CopyFrom(VTmp);
                VTmp.Dispose();
                // Native.TransposeInPlace(V.GetUnsafePtr(), VSize);

                NativeArray<int> FTmp;
                fixed (int* managedVPtr = mesh.triangles)
                    FTmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<int>(
                        (int*) managedVPtr, 3 * FSize, Allocator.Temp);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref FTmp, AtomicSafetyHandle.Create());
#endif

                F.CopyFrom(FTmp);
                FTmp.Dispose();
            }
        }

        void Update()
        {
            if (!libiglJobHandle.HasValue)
            {
                // Perform operation on a worker thread (job)
                if (Input.GetKeyDown(KeyCode.Space))
                    libiglJobHandle = new LibiglJob {V = V, VSize = VSize, F = F, FSize = FSize}.Schedule();
            }
            else if (libiglJobHandle.Value.IsCompleted)
                ApplyModified();
        }

        /// <summary>
        /// Applies the modified mesh by the job
        /// Should only be called when the libiglJob has finished
        /// </summary>
        private void ApplyModified()
        {
            timer.Reset();
            timer.Start();

            // Regain ownership of the data and upload it to the GPU
            libiglJobHandle.Value.Complete(); //Regain ownership on the main thread
            libiglJobHandle = null;
            
//             unsafe
//             {
//                 var V3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(
//                     V.GetUnsafePtr(), VSize, Allocator.None);
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//                 NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V3, AtomicSafetyHandle.Create());
// #endif
//                 mesh.SetVertices(V3);
//             }
            mesh.SetVertexBufferData(V, 0, 0, VSize * 3);

            // mesh.MarkModified();

            timer.Stop();
            Debug.Log("Update Mesh.vertices: " + timer.ElapsedMilliseconds);
        }


        /// <summary>
        /// A class for a C# job to modify the mesh with libigl
        /// This includes all data that a worker thread needs
        /// The Execute() function is called once the job in run
        /// </summary>
        private struct LibiglJob : IJob
        {
            public NativeArray<float> V;
            public NativeArray<int> F;
            public int VSize, FSize;

            public void Execute()
            {
                var jobTimer = new Stopwatch();
                jobTimer.Start();

                // Call our C++ libigl mesh deformation function
                unsafe
                {
                    Native.Harmonic((float*) V.GetUnsafePtr(), VSize, (int*) F.GetUnsafePtr(), FSize);
                }

                jobTimer.Stop();
                Debug.Log($"Harmonic (libigl): {jobTimer.ElapsedMilliseconds}ms");
            }
        }

        private void OnDestroy()
        {
            if (V.IsCreated) V.Dispose();
            if (F.IsCreated) F.Dispose();
        }
    }
}