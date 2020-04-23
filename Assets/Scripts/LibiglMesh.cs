using System;
using System.Collections.Generic;
using System.Threading;
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
        private MeshFilter _meshFilter;
        private Mesh _mesh;

        private MeshData _data;
        private MeshAction _executingAction;
        
        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool JobRunning() { return _workerThread != null; }

        private readonly Queue<MeshAction> _actionsQueue = new Queue<MeshAction>();
        
        /// <summary>
        /// Called like Update but only when <see cref="JobRunning"/> is false.
        /// </summary>
        public static event Action OnAvailableUpdate = delegate {};

        private void Start()
        {
            // Get a reference to the mesh 
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = _meshFilter.mesh;
            _mesh.MarkDynamic();

            _data = new MeshData(_mesh);
        }

        public void ScheduleAction(MeshAction action)
        {
            if ((_workerThread == null || !_workerThread.IsAlive) && _actionsQueue.Count == 0)
                ExecuteAction(action);
            else if (action.AllowQueueing)
                _actionsQueue.Enqueue(action);
        }

        /// <summary>
        /// Execute operation on a worker thread (job)
        /// </summary>
        private void ExecuteAction(MeshAction action)
        {
            Assert.IsTrue(_workerThread == null || !_workerThread.IsAlive);
            _executingAction = action;
            _executingAction.PreExecute?.Invoke(_data);
            _workerThread = new Thread(() => _executingAction.Execute(_data));
            _workerThread.Start();
        }
        
        private void Update()
        {
            if (_workerThread == null)
            {
                if (_actionsQueue.Count > 0)
                    ExecuteAction(_actionsQueue.Dequeue());
                else
                    OnAvailableUpdate();
            }
            else if (!_workerThread.IsAlive)
            {
                // Regain ownership of the data and upload it to the GPU
                _workerThread.Join();
                _workerThread = null;
                
                _executingAction.PostExecute(_mesh, _data);
                _executingAction = null;
            }
        }

        private void OnDestroy()
        {
            _workerThread?.Abort();
            _data.Dispose();
        }
    }
}