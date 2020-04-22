using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Assertions;
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
        private MeshFilter _meshFilter;
        private Mesh _mesh;

        private MeshData _data;
        private GCHandle _dataHandle;
        private MeshAction _executingAction;
        private GCHandle _executingActionHandle;


        private readonly Queue<MeshAction> _actionsQueue = new Queue<MeshAction>();
        
        private readonly Stopwatch _timer = new Stopwatch();
        private JobHandle? _jobHandle;

        private void Start()
        {
            // Get a reference to the mesh 
            _meshFilter = GetComponent<MeshFilter>();
            _mesh = _meshFilter.mesh;
            _mesh.MarkDynamic();

            _data = new MeshData(_mesh);
            
            _dataHandle = GCHandle.Alloc(_data);
        }

        public void ScheduleAction(MeshAction action)
        {
            if (!_jobHandle.HasValue && _actionsQueue.Count == 0)
                ExecuteAction(action);
            else
                _actionsQueue.Enqueue(action);
        }

        /// <summary>
        /// Execute operation on a worker thread (job)
        /// </summary>
        private void ExecuteAction(MeshAction action)
        {
            Assert.IsFalse(_jobHandle.HasValue);
            _executingAction = action;
            _executingActionHandle = GCHandle.Alloc(_executingAction);
            _jobHandle = new LibiglJob {Data = _dataHandle, Action = _executingActionHandle}.Schedule();
        }
        
        private void Update()
        {
            if (!_jobHandle.HasValue)
            {
                if (_actionsQueue.Count > 0)
                    ExecuteAction(_actionsQueue.Dequeue());
            }
            else if (_jobHandle.Value.IsCompleted)
            {
                // Regain ownership of the data and upload it to the GPU
                _jobHandle.Value.Complete(); //Regain ownership on the main thread
                _jobHandle = null;

                _executingAction.Apply(_mesh, _data);
                _executingAction = null;
                _executingActionHandle.Free();
            }
        }

        /// <summary>
        /// A class for a C# job to modify the mesh with libigl
        /// This includes all data that a worker thread needs
        /// The Execute() function is called once the job in run
        /// </summary>
        private struct LibiglJob : IJob
        {
            // public MeshData Data;
            // public MeshAction Action;
            public GCHandle Data;
            public GCHandle Action;

            public void Execute()
            {
                var jobTimer = new Stopwatch();
                jobTimer.Start();

                ((MeshAction) Action.Target).Execute((MeshData) Data.Target);
                
                jobTimer.Stop();
                Debug.Log($"LibiglJob: {jobTimer.ElapsedMilliseconds}ms");
            }
        }

        private void OnDestroy()
        {
            _jobHandle?.Complete();
            _data?.Dispose();
            if(_dataHandle.IsAllocated) _dataHandle.Free();
            if(_executingActionHandle.IsAllocated) _executingActionHandle.Free();
        }
    }
}