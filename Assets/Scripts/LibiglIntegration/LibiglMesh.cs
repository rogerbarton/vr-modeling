using System;
using System.Collections.Generic;
using System.Threading;
using libigl.Behaviour;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;
using UnityNativeTool;

namespace libigl
{
    /// <summary>
    /// Handles the interface between Unity meshes and libigl.<br/>
    /// This component needs to be attached in the Editor to any GameObject that you want to modify with libigl.<br/>
    /// Any libigl related code is defined in the <see cref="LibiglBehaviour"/> class.
    /// This class only handles the threading and connection with the Unity Mesh components.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LibiglMesh : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        public Mesh Mesh { get; private set; }

        /// <summary>
        /// The Unity Mesh data in RowMajor easily accessible as NativeArrays
        /// </summary>
        public UMeshData DataRowMajor { get; private set; }
        /// <summary>
        /// The libigl behaviour instance that is executing on this mesh
        /// </summary>
        public LibiglBehaviour Behaviour { get; private set; }

        /// <summary>
        /// Expensive operations executed in <see cref="LibiglBehaviour.Execute"/> are done in this thread
        /// </summary>
        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool IsJobRunning() { return _workerThread != null; }

        /// <returns>True if this is the active mesh set by the <see cref="MeshManager"/></returns>
        public bool IsActiveMesh() { return MeshManager.ActiveMesh == this; }

        private void Start()
        {
            name = name.Replace("(Clone)", "").Trim();

            if (!Mesh) // Potentially set in ResetTransformToSpawn
            {
                _meshFilter = GetComponent<MeshFilter>();
                Mesh = _meshFilter.mesh;
            }
            Mesh.MarkDynamic();

            // First copy the Mesh arrays into a RowMajor UMeshData instance
            DataRowMajor = new UMeshData(Mesh);
            // Then create the LibiglBehaviour instance which will create a ColMajor instance of the data in the State
            Behaviour = new LibiglBehaviour(this);

#if UNITY_EDITOR
            DisposeBeforeUnload += Dispose;
#endif
        }

        private void Update()
        {
            // Handle creating and cleaning up after a _workerThread has executed
            // This is where the LibiglBehaviour comes in

            if (_workerThread != null && !_workerThread.IsAlive)
                PostExecuteThread();

            Behaviour.Update();

            if (_workerThread == null)
                ExecuteThread();
        }

        /// <summary>
        /// Creates a thread with the LibiglBehaviour code
        /// <remarks>Assert: <see cref="_workerThread"/> is null (finished and <see cref="PostExecuteThread"/> has been called)</remarks>
        /// </summary>
        private void ExecuteThread()
        {
            Assert.IsTrue(_workerThread == null);

            Behaviour.PreExecute();

            _workerThread = new Thread(() => { Behaviour.Execute(); });
            _workerThread.Name = "LibiglWorker";
            _workerThread.Start();
        }

        /// <summary>
        /// Applies changes and cleans up the threading for re-use once the thread has finished.
        /// <remarks>Assert: <see cref="_workerThread"/> is finished executing.</remarks>
        /// </summary>
        private void PostExecuteThread()
        {
            Assert.IsTrue(!_workerThread.IsAlive);

            _workerThread.Join();
            _workerThread = null;

            Behaviour.PostExecute();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void Dispose()
        {
            // Dispose all Native data (NativeArrays and anything created with 'new' in C++)
            // Note: may be called twice in the editor
            _workerThread?.Abort();
            _workerThread = null;
            DataRowMajor?.Dispose();
            DataRowMajor = null;
            Behaviour?.Dispose();
            Behaviour = null;

#if UNITY_EDITOR
            DisposeBeforeUnload -= Dispose;
#endif
        }

#if UNITY_EDITOR
        public static event Action DisposeBeforeUnload = delegate { };

        [NativeDllBeforeUnloadTrigger]
        public static void OnBeforeDllUnloaded()
        {
            DisposeBeforeUnload();
        }
#endif

        /// <summary>
        /// Move mesh up so it is resting on the spawnPoint, ie min of bounding box is at spawnPoint
        /// </summary>
        /// <param name="mesh">Needed for bounding box</param>
        public void ResetTransformToSpawn()
        {
            if (!Mesh)
            {
                _meshFilter = GetComponent<MeshFilter>();
                Mesh = _meshFilter.mesh;
            }

            var spawn = MeshManager.get.meshSpawnPoint;
            var scale = spawn.localScale;

            var pos = spawn.position;
            pos.y -= Mesh.bounds.min.y * scale.y;

            var t = transform;
            t.position = pos;
            t.localScale = scale;
            t.rotation = spawn.transform.rotation;
        }
    }
}