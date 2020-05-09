using System;
using System.Collections.Generic;
using System.Threading;
using libigl.Behaviour;
using UnityEngine;
using UnityEngine.Assertions;
using UnityNativeTool;

namespace libigl
{
    /// <summary>
    /// An interface for modifying a mesh with libigl
    /// The deformation executed is defined in the LibiglJob.Execute()
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LibiglMesh : MonoBehaviour
    {
        private MeshFilter _meshFilter;
        public Mesh Mesh { get; private set; }

        public UMeshData DataRowMajor { get; private set; }
        public LibiglBehaviour Behaviour { get; private set; }
        
        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool IsJobRunning() { return _workerThread != null; }

        public bool IsActiveMesh() { return MeshManager.ActiveMesh == this; }

        private void Start()
        {
            name = name.Replace("(Clone)","").Trim();
            
            // Get a reference to the mesh 
            _meshFilter = GetComponent<MeshFilter>();
            Mesh = _meshFilter.mesh;
            Mesh.MarkDynamic();

            DataRowMajor = new UMeshData(Mesh);
            Behaviour = new LibiglBehaviour(this);
        }

        private void Update()
        {
            if(_workerThread != null && !_workerThread.IsAlive)
                PostExecute();
            
            Behaviour.Update();
            
            if (_workerThread == null)
            {
                Behaviour.PreExecute();
                _workerThread = new Thread(() =>
                {
                    Behaviour.Execute();
                });
                _workerThread.Name = "LibiglWorker";
                _workerThread.Start();
            }
        }

        private void LateUpdate()
        {
            if (_workerThread != null && !_workerThread.IsAlive)
                PostExecute();
        }

        /// <summary>
        /// Applies changes and cleans up the threading for re-use once the thread has finished.
        /// <remarks>Assert: <see cref="_workerThread"/> is finished executing.</remarks>
        /// </summary>
        private void PostExecute()
        {
            Assert.IsTrue(!_workerThread.IsAlive);
            // Regain ownership of the data and upload it to the GPU
            _workerThread.Join();
            _workerThread = null;

            // Allow the user to apply custom changes to the meshZ
            Behaviour.PostExecute();
        }

        private void OnDestroy()
        {
            _workerThread?.Abort();
            _workerThread = null;
            Behaviour?.Dispose();
            Behaviour = null;
            DataRowMajor?.Dispose();
            DataRowMajor = null;
        }
    }
}