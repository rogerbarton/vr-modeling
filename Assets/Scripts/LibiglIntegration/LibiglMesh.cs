using System;
using System.Collections.Generic;
using System.Threading;
using libigl.Behaviour;
using UnityEngine;
using UnityEngine.Assertions;

namespace libigl
{
    /// <summary>
    /// An interface for modifying a mesh with libigl
    /// The deformation executed is defined in the LibiglJob.Execute()
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class LibiglMesh : MonoBehaviour
    {
        public MeshFilter meshFilter;
        public Mesh Mesh { get; private set; }

        public MeshData DataRowMajor { get; private set; }
        public LibiglBehaviour Behaviour;
        
        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool IsJobRunning() { return _workerThread != null; }

        private void Start()
        {
            name = name.Replace("(Clone)","").Trim();
            
            // Get a reference to the mesh 
            meshFilter = GetComponent<MeshFilter>();
            Mesh = meshFilter.mesh;
            Mesh.MarkDynamic();

            DataRowMajor = new MeshData(Mesh);
            Behaviour = new LibiglBehaviour(this);
        }

        private void Update()
        {
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
            else if (!_workerThread.IsAlive)
                PostExecute();
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
            Behaviour.Dispose();
            DataRowMajor.Dispose();
        }
    }
}