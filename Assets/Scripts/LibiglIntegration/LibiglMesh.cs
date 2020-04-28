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

        [NonSerialized] public MeshData Data;
        [NonSerialized] public MeshData DataRowMajor;
        private MeshAction[] _executingActions;
        private readonly Queue<MeshAction> _actionsQueue = new Queue<MeshAction>();

        private Thread _workerThread;
        /// <returns>True if a job/worker thread is running on the MeshData</returns>
        public bool JobRunning() { return _workerThread != null; }


        private void Start()
        {
            // Get a reference to the mesh 
            _meshFilter = GetComponent<MeshFilter>();
            Mesh = _meshFilter.mesh;
            Mesh.MarkDynamic();

            Data = new MeshData(Mesh);
            DataRowMajor = new MeshData(Mesh, true);
        }

        /// <summary>
        /// Schedules the action once, adds it to a queue of actions to be executed 
        /// </summary>
        public void ScheduleAction(MeshAction action)
        {
            if(!_actionsQueue.Contains(action))
                _actionsQueue.Enqueue(action);
        }

        /// <summary>
        /// Execute operation on a worker thread (job)
        /// </summary>
        private void ExecuteActions()
        {
            Assert.IsTrue(_workerThread == null || !_workerThread.IsAlive);
            _executingActions = _actionsQueue.ToArray();
            _actionsQueue.Clear();
            
            foreach (var action in _executingActions)
                action.PreExecute?.Invoke(DataRowMajor);
            
            // Execute the actions with ColMajor data and then apply the changes to the DataRowMajor
            _workerThread = new Thread(() =>
            {
                DataRowMajor.ApplyDirtyToTranspose(Data);
                foreach (var action in _executingActions)
                    action.Execute(Data);
                Data.ApplyDirtyToTranspose(DataRowMajor);
            });
            _workerThread.Start();
        }

        private void Update()
        {
            if (_workerThread == null)
            {
                if (_actionsQueue.Count > 0)
                    ExecuteActions();
            }
            else if (!_workerThread.IsAlive)
            {
                // Regain ownership of the data and upload it to the GPU
                _workerThread.Join();
                _workerThread = null;

                // Allow the user to apply custom changes to the mesh, give the RowMajor version that has been updated
                foreach (var action in _executingActions) 
                    action.PostExecute?.Invoke(Mesh, DataRowMajor);
                DataRowMajor.ApplyChangesToMesh(Mesh);
                _executingActions = null;
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