using System;
using UI;
using UnityEngine;
using XrInput;
using Object = UnityEngine.Object;

namespace Libigl
{
    /// <summary>
    /// This is where the behaviour that interfaces with libigl is. It follows a Pre/Post/Execute threading pattern.
    /// This is a <c>partial</c> class, meaning it is split between several files.
    /// <remarks>See also <see cref="Libigl.LibiglMesh"/> which handles the threading and calls the Pre/Post/Execute callbacks</remarks>
    /// </summary>
    public unsafe partial class LibiglBehaviour : IDisposable
    {
        /// <summary>
        /// A <b>pointer</b> to the C++ state.
        /// This is allocated and deleted in C++ within <see cref="Native.InitializeMesh"/> and <see cref="Native.DisposeMesh"/>.
        /// </summary>
        public State* State;


        /// <summary>
        /// The input state on the main thread. This is copied to the thread input state <c>State.Input</c> at the end of PreExecute
        /// and is then immediately consumed by <see cref="ConsumeInput"/>.
        /// </summary>
        public InputState Input;
        public InputState ExecuteInput;

        /// <summary>
        /// Reference to the <see cref="Libigl.LibiglMesh"/> used to apply changes to the <see cref="Libigl.LibiglMesh.DataRowMajor"/> and the Unity <see cref="Mesh"/>
        /// </summary>
        public readonly LibiglMesh LibiglMesh;

        private readonly UiDetails _uiDetails;

        public LibiglBehaviour(LibiglMesh libiglMesh)
        {
            LibiglMesh = libiglMesh;
            
            // Initialize C++ and create the State from the DataRowMajor
            State = Native.InitializeMesh(libiglMesh.DataRowMajor.GetNative(), LibiglMesh.name);
            Input = InputState.GetInstance();
            
            _uiDetails = UiManager.get.CreateDetailsPanel();
            _uiDetails.Initialize(this);
        }

        /// <summary>
        /// Called every frame, the normal Unity Update. Use this to update UI, input responsively.
        /// Do not call any expensive libigl methods here, use Execute instead<br/>
        /// Be careful not to modify the shared state if there is a job running <see cref="Libigl.LibiglMesh.IsJobRunning"/>.
        /// Consider making a copy of certain data, using <see cref="PreExecute"/> or using atomics/Interlocked.
        /// Update is called just before <see cref="PreExecute"/>.
        /// </summary>
        public void Update()
        {
            if (!LibiglMesh.IsActiveMesh()) return;
            
            UpdateTransform();
            UpdateInput();
        }
        
        /// <summary>
        /// Called just before a new thread is started in which <see cref="Execute"/> is called.
        /// Use this to update the input state, set flags and access any Unity API from the main thread.<br/>
        /// Called on the main thread.
        /// </summary>
        public void PreExecute()
        {
            // Add logic here that uses the Unity API (e.g. Input)
            PreExecuteInput();
            PreExecuteTransform();
            
            // Apply changes in UI to the state
            _uiDetails.UpdatePreExecute();
            
            // Copy the Input to ExecuteInput so the thread has its own copy
            ExecuteInput = Input;
            // Immediately consume the input on the main thread copy so we can detect new changes whilst we are in Execute
            ConsumeInput();
            ConsumeTransform();
        }

        /// <summary>
        /// Perform expensive computations here. This is called similarly to Update.<br/>
        /// Called on a worker thread from which any Unity API function, with a few exceptions such as <c>Debug.Log</c>, cannot be called.
        /// <remarks>There is one worker thread per <see cref="Libigl.LibiglMesh"/>.<br/>
        /// You should call <c>_libiglMesh.DataRowMajor.ApplyDirty(_state)</c> here to apply changes to the
        /// RowMajor <see cref="UMeshData"/> outside the main thread.</remarks>
        /// </summary>
        public void Execute()
        {
            ActionUi();
            if(LibiglMesh.IsActiveMesh())
            {
                switch (ExecuteInput.Shared.ActiveTool)
                {
                    case ToolType.Transform:
                        break;
                    case ToolType.Select:
                        ActionSelect();
                        ActionTransformSelection();
                        break;
                }

                ActionHarmonic();
                ActionArap();
            }
            
            // Apply changes back to the RowMajor so they can be applied to the mesh
            LibiglMesh.DataRowMajor.ApplyDirty(State, ExecuteInput);
        }

        /// <summary>
        /// Called after <see cref="Execute"/> to apply changes to the mesh.<br/>
        /// Called on the main thread.
        /// <remarks>Use <c>LibiglMesh.DataRowMajor.ApplyDirtyToMesh</c> to apply changes</remarks>
        /// </summary>
        public void PostExecute()
        {
            _uiDetails.UpdatePostExecute();
            
            // Apply RowMajor changes to the Unity mesh, this must be done with RowMajor data
            LibiglMesh.DataRowMajor.ApplyDirtyToMesh(LibiglMesh.Mesh);
            
            if ((State->DirtyState & DirtyFlag.VDirty) > 0 && (State->DirtyState & DirtyFlag.DontComputeBounds) == 0)
                LibiglMesh.UpdateBoundingBoxSize();
            
            // Consume Dirty
            State->DirtyState = DirtyFlag.None;
            State->DirtySelections = 0;
            State->DirtySelectionsResized = 0;
        }

        public void Dispose()
        {
            // Be sure to dispose of any NativeArrays that are not garbage collected
            if(_uiDetails)
                Object.Destroy(_uiDetails.gameObject);
            
            // Delete the State fully inside C++
            Native.DisposeMesh(State);
            State = null;
        }
    }
}