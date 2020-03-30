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
    /// The deformation executed is defined in the VertexJob.Execute()
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LibiglMesh : MonoBehaviour
    {
        private MeshFilter meshFilter;
        private Mesh mesh;
        private NativeArray<float> V;
        private int VSize;

        Stopwatch timer = new Stopwatch();
        JobHandle? translateJobHandle;

        void Start()
        {
            // Get a reference to the mesh 
            meshFilter = GetComponent<MeshFilter>();
            mesh = meshFilter.mesh;
            mesh.MarkDynamic(); //Allow us to modify it at runtime

            // Create a vertex data copy as a NativeArray<float> which we will pass to libigl
            VSize = mesh.vertexCount;
            V = new NativeArray<float>(3 * VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            // Before we can use this we need to add a safety handle (only in the editor)
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
#endif

            unsafe
            {
                NativeArray<float> tmp;
                fixed (Vector3* managedVPtr = mesh.vertices)
                    tmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>(
                        (float*) managedVPtr, 3 * VSize, Allocator.Temp);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref tmp, AtomicSafetyHandle.Create());
#endif

                V.CopyFrom(tmp);
                tmp.Dispose();
            }
        }

        void Update()
        {
            //If the translate job has finished, regain ownership of the data and upload it to the GPU
            if (translateJobHandle.HasValue && translateJobHandle.Value.IsCompleted)
            {
                translateJobHandle.Value.Complete(); //Regain ownership
                translateJobHandle = null;
                timer.Reset();
                timer.Start();
                unsafe
                {
                    var V3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(
                        V.GetUnsafePtr(), VSize, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V3, AtomicSafetyHandle.Create());
#endif

                    mesh.SetVertices(V3);
                }

                mesh.MarkModified(); //recalculate bounding boxes etc
                mesh.UploadMeshData(false);

                timer.Stop();
                Debug.Log("Update Mesh.vertices: " + timer.ElapsedMilliseconds);
            }

            if (Input.GetAxis("Horizontal") != 0f)
                TranslateMesh(new Vector3(Time.deltaTime * Input.GetAxis("Horizontal"), 0, 0));
        }

        /// <summary>
        /// A class for a C# job to translate the mesh required for running on a separate thread
        /// This includes all data that the thread needs and the Execute() function which is called once the job in scheduled
        /// </summary>
        private struct VertexJob : IJob
        {
            public NativeArray<float> V;
            public int VSize;
            public Vector3 direction;

            public void Execute()
            {
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Call our C++ libigl mesh deformation function
                unsafe
                {
                    Native.TranslateMesh((float*) V.GetUnsafePtr(), VSize, direction);
                }

                timer.Stop();
                Debug.Log("Modify Mesh Data (libigl): " + timer.ElapsedMilliseconds);
            }
        }

        private void TranslateMesh(Vector3 direction)
        {
            //Dont schedule another job if the current one has not finished yet, or the last change still has to be uploaded
            if (translateJobHandle.HasValue) return;

            //Perform operation on a worker thread (job)
            translateJobHandle = new VertexJob {V = V, VSize = VSize, direction = direction}.Schedule();
        }

        private void OnDestroy()
        {
            if (V.IsCreated) V.Dispose();
        }
    }
}