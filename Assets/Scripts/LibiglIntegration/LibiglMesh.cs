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
        public Mesh Mesh { get; private set; }

        public MeshData Data;
        public MeshData DataRowMajor;
        private MeshAction _executingAction;

        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool JobRunning() { return _workerThread != null; }

        private readonly Queue<MeshAction> _actionsQueue = new Queue<MeshAction>();

        /// <summary>
        /// Called like Update but only when <see cref="JobRunning"/> is false.
        /// </summary>
        public event Action OnAvailableUpdate = delegate { };

        private void Start()
        {
            // Get a reference to the mesh 
            _meshFilter = GetComponent<MeshFilter>();
            Mesh = _meshFilter.mesh;
            Mesh.MarkDynamic();

            Data = new MeshData(Mesh);
            DataRowMajor = new MeshData(Mesh, true);
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
            _executingAction.PreExecute?.Invoke(DataRowMajor);
            
            // Execute the action with ColMajor data and then apply the changes to the DataRowMajor
            _workerThread = new Thread(() =>
            {
                DataRowMajor.ApplyDirtyToTranspose(Data);
                _executingAction.Execute(Data);
                Data.ApplyDirtyToTranspose(DataRowMajor);
            });
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

                // Allow the user to apply custom changes to the mesh, give the RowMajor version that has been updated
                _executingAction.PostExecute?.Invoke(Mesh, DataRowMajor);
                DataRowMajor.ApplyChangesToMesh(Mesh);
                _executingAction = null;
            }
        }

        private void OnDestroy()
        {
            _workerThread?.Abort();
            Data.Dispose();
            DataRowMajor.Dispose();
        }
    }
}