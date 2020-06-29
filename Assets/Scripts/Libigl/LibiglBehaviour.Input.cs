using System;
using UnityEngine;
using XrInput;

namespace Libigl
{
    public partial class LibiglBehaviour
    {
        // C# only input variables
        private Vector2 _lastPrimaryAxisValueL;
        
        private void UpdateInput()
        {
            switch (InputManager.State.ActiveTool)
            {
                case ToolType.Transform:
                    UpdateInputTransform();
                    break;
                case ToolType.Select:
                    UpdateInputSelect();
                    break;
            }
            
            if(InputManager.get.BrushL)
                Input.BrushPosL = LibiglMesh.transform.InverseTransformPoint(InputManager.get.BrushL.center.position);
            if(InputManager.get.BrushR)
                Input.BrushPosR = LibiglMesh.transform.InverseTransformPoint(InputManager.get.BrushR.center.position);
        }

        /// <summary>
        /// Input for the transform tool
        /// </summary>
        private void UpdateInputTransform()
        {
            // Change the transform tool mode
            if (_doTransformL)
                InputManager.State.ToolTransformMode = _doTransformR ? ToolTransformMode.TransformingLr : ToolTransformMode.TransformingL;
            else if (_doTransformR)
                InputManager.State.ToolTransformMode = ToolTransformMode.TransformingR;
            else
                InputManager.State.ToolTransformMode = ToolTransformMode.Idle;
        }

        /// <summary>
        /// Gathering input for the select tool
        /// </summary>
        private void UpdateInputSelect()
        {
            // Update the tool sub state/mode
            if (_doTransformL)
                InputManager.State.ToolSelectMode = _doTransformR ? ToolSelectMode.TransformingLr : ToolSelectMode.TransformingL;
            else if (_doTransformR)
                InputManager.State.ToolSelectMode = ToolSelectMode.TransformingR;
            else if (InputManager.State.TriggerL > PressThres || InputManager.State.TriggerR > PressThres)
            {
                InputManager.State.ToolSelectMode = ToolSelectMode.Selecting;
                
                if (InputManager.State.TriggerL > PressThres && 
                    (InputManager.State.ActiveSelectionMode != SelectionMode.Invert || InputManager.StatePrev.TriggerL < PressThres)
                    && InputManager.get.BrushL.InsideActiveMeshBounds)
                    Input.DoSelectL = true;
                
                if (InputManager.State.TriggerR > PressThres && 
                    (InputManager.State.ActiveSelectionMode != SelectionMode.Invert || InputManager.StatePrev.TriggerR < PressThres)
                    && InputManager.get.BrushR.InsideActiveMeshBounds)
                    Input.DoSelectR = true;

                Input.AlternateSelectModeL = InputManager.State.PrimaryBtnL;
                Input.AlternateSelectModeR = InputManager.State.PrimaryBtnR;
            }
            else
                InputManager.State.ToolSelectMode = ToolSelectMode.Idle;
            
            // Change the selection with the right hand primary2DAxis.x
            if (Mathf.Abs(InputManager.State.PrimaryAxisR.x) > 0.05f && Mathf.Abs(InputManager.StatePrev.PrimaryAxisR.x) < 0.05f)
                SetActiveSelectionIncrement((int) Mathf.Sign(InputManager.State.PrimaryAxisR.x));
        }

        private void PreExecuteInput()
        {
            Input.SharedPrev = Input.Shared;
            Input.Shared = InputManager.State;
        }
        
        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            Input.DoSelectLPrev = Input.DoSelectL;
            Input.DoSelectL = false;
            Input.DoSelectRPrev = Input.DoSelectR;
            Input.DoSelectR = false;
            Input.DoClearSelection = 0;
            Input.VisibleSelectionMaskChanged = false;
            if(!Input.DoHarmonicRepeat)
                Input.DoHarmonic = false;
            if(!Input.DoArapRepeat)
                Input.DoArap = false;
            Input.ResetV = false;
        }

        /// <summary>
        /// Invoked when the active selection of the mesh has changed
        /// </summary>
        public event Action OnActiveSelectionChanged = delegate { };
        
        public void SetActiveSelection(int value)
        {
            if(Input.ActiveSelectionId == value) return;
            
            Input.ActiveSelectionId = value;
            OnActiveSelectionChanged();
        }
        
        /// <summary>
        /// Increment the active selection and safely loops
        /// </summary>
        public void SetActiveSelectionIncrement(int increment)
        {
            SetActiveSelection((int) ((Input.ActiveSelectionId + increment + Input.SCountUi) % Input.SCountUi));
        }
    }
}