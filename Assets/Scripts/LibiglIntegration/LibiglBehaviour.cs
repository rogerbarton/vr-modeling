using System;
using UnityEngine;
using UnityNativeTool;

namespace libigl.Behaviour
{
    /// <summary>
    /// This is where the behaviour that interfaces with libigl is. It follow a Pre/Post/Execute threading pattern.
    /// </summary>
    public unsafe partial class LibiglBehaviour : IDisposable
    {
        /// <summary>
        /// Column major mesh data storing the arrays that will be applied to the Unity mesh
        /// </summary>
        private State* _state;
        private readonly LibiglMesh _libiglMesh;

        public LibiglBehaviour(LibiglMesh libiglMesh)
        {
            _libiglMesh = libiglMesh;
            
            // Initialize C++
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
        
        public void PreExecute()
        {
            // Add logic here that uses the Unity API (e.g. Input)
            _actionTranslate = Input.GetKeyDown(KeyCode.W);
            _actionSelect = Input.GetMouseButtonDown(0);
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

        public void Execute()
        {
            // Add your logic here
            ActionTranslate();
            ActionSelect();
            ActionHarmonic();
            
            // Apply changes back to the RowMajor so they can be applied to the mesh
            _libiglMesh.DataRowMajor.ApplyDirty(_state);
        }

        public void PostExecute()
        {
            // Apply RowMajor changes to the Unity mesh, this must be done with RowMajor data
            _libiglMesh.DataRowMajor.ApplyDirtyToMesh(_libiglMesh.Mesh);
        }

        public void Dispose()
        {
            // Be sure to dispose of any NativeArrays that are not garbage collected
            Native.DisposeMesh(_state);
            _state = null;
        }
    }
}