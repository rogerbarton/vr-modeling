using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace Testing
{
    /// <summary>
    /// This script was used for initial testing, also with the C++ interface and has not been used for a long time.
    /// May be useful for reference.
    /// 
    /// Experimentation script for how to apply changes to the mesh from libigl to the Unity mesh.
    /// This mainly focuses on the newer mesh API.
    /// There are three ways of applying our vertex matrix V:
    /// 1. Use C# wrappers mesh.SetVertices(), possibly quite inefficient
    /// 2. Use the newer mesh API to write to the buffer via C#
    /// 3. Use the GPU pointer to write to the buffer directly via C++ (quite advanced and complicated but may be very fast)
    ///
    /// It is important to consider the VertexBufferLayout <see cref="Libigl.Native.VertexBufferLayout"/>
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class MeshApiTest : MonoBehaviour
    {
        public bool useCustomUploadToGpu;

        [StructLayout(LayoutKind.Sequential)]
        public unsafe class VertexUploadData
        {
            public int changed; // bool not blittable
            public float* gfxVertexBufferPtr;
            public float* VPtr;
            public int VSize;
        }

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private NativeArray<float> V;
        private int VSize;

        private static VertexUploadData _vertexUploadData;
        private static GCHandle _vertexUploadHandle;
        private CommandBuffer _commandBuffer; // A list of stuff to do on the render thread at a specific point in time,
        private IntPtr _gfxVertexBufferPtr;


        void Start()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = _meshFilter.mesh;
            VSize = _mesh.vertexCount;
            V = new NativeArray<float>(3 * VSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V, AtomicSafetyHandle.Create());
#endif
            var layout = _mesh.GetVertexAttributes();
            _mesh.MarkDynamic(); //GPU buffer should be dynamic so we can modify it easily with D3D11Buffer::Map()
            unsafe
            {
                NativeArray<float> tmp;
                fixed (Vector3* managedVPtr = _mesh.vertices)
                    tmp = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float>((float*) managedVPtr, 3 * VSize,
                        Allocator.Temp);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref tmp, AtomicSafetyHandle.Create());
#endif
                // NativeArray<float>.Copy(tmp, V); //Make our own copy
                V.CopyFrom(tmp);

                tmp.Dispose();
            }

            // Do not use this as it makes the GPU buffer no longer dynamic for us
            // mesh.UploadMeshData(true); //delete managed copy and make no longer readable via Unity

            if (useCustomUploadToGpu)
            {
                //Setup custom uploading mesh data
                _vertexUploadData = new VertexUploadData();
                _vertexUploadHandle = GCHandle.Alloc(_vertexUploadData, GCHandleType.Pinned);
                _gfxVertexBufferPtr = _mesh.GetNativeVertexBufferPtr(0);

                // command is called every frame until it's removed (can leave it if updating every frame)
                _commandBuffer = new CommandBuffer();
                _commandBuffer.name = "UploadMeshCmd"; // for profiling
                IntPtr dataPtr = _vertexUploadHandle.AddrOfPinnedObject();
                // command.IssuePluginEventAndData(Native.GetUploadMeshPtr(), 1, dataPtr);
                // Camera.main.AddCommandBufferAsync(CameraEvent.AfterEverything, command, ComputeQueueType.Default);
                // Graphics.ExecuteCommandBufferAsync(command, ComputeQueueType.Default);
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
                if (useCustomUploadToGpu)
                {
                    // TODO
                    // Upload to GPU is done on the render thread at the end of a frame render, this is done via CommandBuffer

                    // Can we assume that if we added the command in the last frame that it has been performed -> yes?
                    // otherwise check that the last upload has been completed

                    // Setup data, essentially the params we pass
                    // Alternatively call a function now passing the pointers, like SetTimeFromUnity in the sample
                    unsafe
                    {
                        _vertexUploadData.changed = 1;
                        _vertexUploadData.gfxVertexBufferPtr = (float*) _gfxVertexBufferPtr.ToPointer();
                        if (_gfxVertexBufferPtr != _mesh.GetNativeVertexBufferPtr(0))
                            Debug.LogError("gfx vert buffer ptr changed!");
                        _vertexUploadData.VPtr = (float*) V.GetUnsafePtr();
                        _vertexUploadData.VSize = VSize;
                        // Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, command);
                        // mesh.MarkModified();
                        // TODO: remove when no longer updating mesh every frame, or store in the VertexUploadData if the data has changed or not.
                        // Call this based on user Input.OnKeyUp etc
                        // Camera.main.RemoveCommandBuffer(CameraEvent.AfterEverything, command);
                    }
                }
                else
                {
                    unsafe
                    {
                        var V3 = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<Vector3>(V.GetUnsafePtr(), VSize,
                            Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                        NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref V3, AtomicSafetyHandle.Create());
#endif
                        _mesh.SetVertices(V3);
                    }

                    //Set vertexbufferdata?
                    _mesh.MarkModified(); //recalculate bounding boxes etc
                    _mesh.UploadMeshData(false);
                }

                timer.Stop();
                Debug.Log("Upload+GetPtr schedule: " + timer.ElapsedMilliseconds);
            }

            if (Input.GetAxis("Horizontal") != 0f)
                TranslateMesh(new Vector3(Time.deltaTime * Input.GetAxis("Horizontal"), 0, 0));
        }

        private struct VertexJob : IJob
        {
            public NativeArray<float> V;
            public int VSize;
            public Vector3 direction;

            public void Execute()
            {
                unsafe
                {
                    // Native.TranslateMesh((float*) V.GetUnsafePtr(), VSize, direction);
                }
            }
        }

        Stopwatch timer = new Stopwatch();
        JobHandle? translateJobHandle;

        private void TranslateMesh(Vector3 direction)
        {
            //Dont schedule another job if the current one has not finished yet, or the last change still has to be uploaded
            if (translateJobHandle.HasValue /* || vertexUploadData.changed > 0*/)
                return;

            //Perform operation on a worker thread (job)
            var job = new VertexJob {V = V, VSize = VSize, direction = direction};
            translateJobHandle = job.Schedule();
        }

        /// <summary>
        /// Test function that creates a new mesh with a cube
        /// </summary>
        private void CreateCube()
        {
            var mesh = new Mesh();
            mesh.name = "generated-mesh";
            _meshFilter.mesh = mesh;
            //mesh.MarkDynamic(); //if we want to repeatedly modify the mesh

            var VLayout = new[]
            {
                //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
                new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
            };
            var VCount = 8;
            mesh.SetVertexBufferParams(VCount, VLayout); //Note:sizeof one vertex is defined in the layout as 3*4B
            //So buffer size is vertices * 3 * 4B

            var FCount = 12;
            mesh.SetIndexBufferParams(3 * FCount,
                IndexFormat.UInt32); //Note: Size of buffer is count * uint32 = 3 * faces*4B

            var V = new NativeArray<float>(3 * VCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            var F = new NativeArray<uint>(3 * FCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            //Or Allocator.Persistent with Dispose()

            //--- Call C++ to fill V, F
            unsafe
            {
                // Native.FillMesh(V.GetUnsafePtr(), VCount, F.GetUnsafePtr(), FCount);
            }

            //Be sure to multiply by 3 to copy whole array
            mesh.SetVertexBufferData(V, 0, 0, 3 * VCount, 0);
            mesh.SetIndexBufferData(F, 0, 0, 3 * FCount);
            //Note! Set*BufferData incurs a memcpy

            //Create one submesh which will be rendered (required)
            mesh.subMeshCount = 1;
            mesh.SetSubMesh(0, new SubMeshDescriptor(0, 3 * FCount));

            //Not sure if this is needed
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.UploadMeshData(true); //for optimization, cannot edit/view in debugger after issuing this

            mesh.MarkModified(); //As we DontNotifyMeshUsers



            //--- To update an existing initialized mesh
            //Use this to get pointers to buffers, can pass IntPtr to a float* or int* in c++
            // IntPtr voidPtrToVArr = mesh.GetNativeVertexBufferPtr(0); //retrieve the first buffer
            // IntPtr voidPtrToFArr = mesh.GetNativeIndexBufferPtr();

            // V.Dispose();
            // F.Dispose();
        }

        private void OnDestroy()
        {
            if (V.IsCreated)
                V.Dispose();
            if (_vertexUploadHandle.IsAllocated)
                _vertexUploadHandle.Free();
        }
    }
}
