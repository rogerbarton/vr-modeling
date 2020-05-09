using System;
using UnityEngine;

namespace libigl.Behaviour
{
    /// <summary>
    /// This is where the behaviour that interfaces with libigl is. It follows a Pre/Post/Execute threading pattern.
    /// This is a <c>partial</c> class, meaning it is split between several files.
    /// <remarks>See also <see cref="LibiglMesh"/> which handles the threading and calls the Pre/Post/Execute callbacks</remarks>
    /// </summary>
    public unsafe partial class LibiglBehaviour : IDisposable
    {
        /// <summary>
        /// A <b>pointer</b> to the C++ state.
        /// This is allocated and deleted in C++ within <see cref="Native.InitializeMesh"/> and <see cref="Native.DisposeMesh"/>.
        /// </summary>
        private State* _state;
        
        /// <summary>
        /// Reference to the <see cref="LibiglMesh"/> used to apply changes to the <see cref="LibiglMesh.DataRowMajor"/> and the Unity <see cref="LibiglMesh.Mesh"/>
        /// </summary>
        private readonly LibiglMesh _libiglMesh;

        public LibiglBehaviour(LibiglMesh libiglMesh)
        {
            _libiglMesh = libiglMesh;
            
            // Initialize C++ and create the State from the DataRowMajor
            _state = Native.InitializeMesh(libiglMesh.DataRowMajor.GetNative(), _libiglMesh.name);
        }

        /// <summary>
        /// Called every frame, the normal Unity Update. Use this to update UI, input responsively.
        /// Do not call any expensive libigl methods here, use Execute instead<br/>
        /// Be careful not to modify the shared state if there is a job running <see cref="LibiglMesh.IsJobRunning"/>.
        /// Consider making a copy of certain data, using <see cref="PreExecute"/> or using atomics/Interlocked.
        /// Update is called just before <see cref="PreExecute"/>.
        /// </summary>
        public void Update()
        {
            
        }
        
        /// <summary>
        /// Called just before a new thread is started in which <see cref="Execute"/> is called.
        /// Use this to update the input state, set flags and access any Unity API from the main thread.<br/>
        /// Called on the main thread.
        /// </summary>
        public void PreExecute()
        {
            // Add logic here that uses the Unity API (e.g. Input)
            _actionTranslate |= Input.GetKeyDown(KeyCode.W);
            _actionSelect |= Input.GetMouseButtonDown(0);
            if (_actionSelect)
            {
                _actionSelectPos = Input.mousePosition;
            }
            
            if (Math.Abs(Input.mouseScrollDelta.y) > 0.01f)
            {
                _actionSelectRadiusSqr += 0.1f * Input.mouseScrollDelta.y;
                Mathf.Clamp(_actionSelectRadiusSqr, 0.025f, 1f);
            }
        }

        /// <summary>
        /// Perform expensive computations here. This is called similarly to Update.<br/>
        /// Called on a worker thread from which any Unity API function, with a few exceptions such as <c>Debug.Log</c>, cannot be called.
        /// <remarks>There is one worker thread per <see cref="LibiglMesh"/>.<br/>
        /// You should call <c>_libiglMesh.DataRowMajor.ApplyDirty(_state)</c> here to apply changes to the
        /// RowMajor <see cref="UMeshData"/> outside the main thread.</remarks>
        /// </summary>
        public void Execute()
        {
            // Add your logic here
            ActionTranslate();
            ActionSelect();
            ActionHarmonic();
            
            // Apply changes back to the RowMajor so they can be applied to the mesh
            _libiglMesh.DataRowMajor.ApplyDirty(_state);
        }

        /// <summary>
        /// Called after <see cref="Execute"/> to apply changes to the mesh.<br/>
        /// Called on the main thread.
        /// <remarks>Use <c>LibiglMesh.DataRowMajor.ApplyDirtyToMesh</c> to apply changes</remarks>
        /// </summary>
        public void PostExecute()
        {
            // Apply RowMajor changes to the Unity mesh, this must be done with RowMajor data
            _libiglMesh.DataRowMajor.ApplyDirtyToMesh(_libiglMesh.Mesh);
        }

        public void Dispose()
        {
            // Be sure to dispose of any NativeArrays that are not garbage collected
            
            // Delete the C++ state
            Native.DisposeMesh(_state);
            _state = null;
        }
    }
}