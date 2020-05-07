using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Windows.Speech;
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
        private readonly MeshData _data;
        private State* _state;

        // Action Related
        private bool _actionTranslate;
        
        private bool _actionSelect;
        private Vector3 _actionSelectPos;
        private float _actionSelectRadius;

        public LibiglBehaviour(LibiglMesh libiglMesh)
        {
            // Create a ColMajor copy of the data
            _data = new MeshData(libiglMesh.DataRowMajor);
            
            // Initialize C++
            _state = Native.InitializeMesh(_data.GetNative(), libiglMesh.name);
        }

        /// <summary>
        /// Called every frame, the normal Unity Update
        /// Use this to update UI, input responsively
        /// Do not call any expensive libigl methods here, use Execute instead
        /// </summary>
        public void Update()
        {
            
        }
        
        public void PreExecute(LibiglMesh libiglMesh)
        {
            // Add logic here that uses the Unity API (e.g. Input)
            _actionTranslate = Input.GetKeyDown(KeyCode.W);
            _actionSelect = Input.GetMouseButton(0);
            if (_actionSelect)
            {
                _actionSelectPos = Input.mousePosition;
                _actionSelectRadius = 1f;
            }
        }

        public void Execute(LibiglMesh libiglMesh)
        {
            // Apply changes to ColMajor, only if the RowMajor is modified outside Execute()
            libiglMesh.DataRowMajor.ApplyDirtyToTranspose(_data);

            // Add your logic here
            ActionTranslate();
            ActionSelect();
            ActionHarmonic();
            
            // Apply changes back to the RowMajor so they can be applied to the mesh
            _data.ApplyDirtyToTranspose(libiglMesh.DataRowMajor);
        }

        public void PostExecute(LibiglMesh libiglMesh)
        {
            // Apply RowMajor changes to the Unity mesh, this must be done with RowMajor data
            libiglMesh.DataRowMajor.ApplyDirtyToMesh(libiglMesh.Mesh);
        }

        public void Dispose()
        {
            // Be sure to dispose of any NativeArrays that are not garbage collected
            _data?.Dispose();
            Native.DisposeMesh(_state);
            _state = null;
        }
    }
}