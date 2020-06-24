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
        }

        /// <summary>
        /// Input for the transform tool
        /// </summary>
        private void UpdateInputTransform()
        {
            // Change the transform tool mode
            if (_doTransformL)
                InputManager.State.ToolTransformMode = _doTransformR ? ToolTransformMode.TransformingLR : ToolTransformMode.TransformingL;
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
            if (_doTransformL)
                InputManager.State.ToolSelectMode = _doTransformR ? ToolSelectMode.TransformingLR : ToolSelectMode.TransformingL;
            else if (_doTransformR)
                InputManager.State.ToolSelectMode = ToolSelectMode.TransformingR;
            else if (InputManager.StatePrev.TriggerR > PressThres)
                InputManager.State.ToolSelectMode = ToolSelectMode.Selecting;
            else
                InputManager.State.ToolSelectMode = ToolSelectMode.Idle;
            
            // Change the selection with the right hand primary2DAxis.x
            if (Mathf.Abs(InputManager.State.PrimaryAxisR.x) > 0.05f && Mathf.Abs(InputManager.StatePrev.PrimaryAxisR.x) < 0.05f)
                ChangeActiveSelection((int) Mathf.Sign(InputManager.State.PrimaryAxisR.x));

            if (!(_doTransformL || _doTransformR) && InputManager.State.TriggerR > PressThres)
            {
                if (InputManager.State.ActiveSelectionMode != SelectionMode.Invert || InputManager.StatePrev.TriggerR < PressThres)
                {
                    Input.DoSelect = true;
                    Input.BrushPos = LibiglMesh.transform.InverseTransformPoint(InputManager.get.BrushR.center.position);
                }
            }
        }
        
        public void ChangeActiveSelection(int increment)
        {
            _uiDetails.SetActiveSelection((int) ((Input.ActiveSelectionId + increment + Input.SCountUi) % Input.SCountUi));
        }

        private void PreExecuteInput()
        {
            Input.SharedPrev = Input.Shared;
            Input.Shared = InputManager.State;
            
            Input.DoTransform |= UnityEngine.Input.GetKeyDown(KeyCode.W);
            Input.DoSelect |= UnityEngine.Input.GetMouseButtonDown(0);
        }
        
        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            Input.DoSelectPrev = Input.DoSelect;
            Input.DoSelect = false;
            Input.DoClearSelection = 0;
            Input.VisibleSelectionMaskChanged = false;
            if(!Input.DoHarmonicRepeat)
                Input.DoHarmonic = false;
            if(!Input.DoArapRepeat)
                Input.DoArap = false;
            Input.ResetV = false;
        }
    }
}