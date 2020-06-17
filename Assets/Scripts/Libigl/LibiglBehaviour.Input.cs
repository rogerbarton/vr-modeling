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
            if (!LibiglMesh.IsActiveMesh()) return;

            switch (InputManager.Input.ActiveTool)
            {
                case ToolType.Default:
                    UpdateInputDefault();
                    break;
                case ToolType.Select:
                    UpdateInputSelect();
                    break;
            }
        }

        /// <summary>
        /// Input for the default tool
        /// </summary>
        private void UpdateInputDefault()
        {
        }

        /// <summary>
        /// Gathering input for the select tool
        /// </summary>
        private void UpdateInputSelect()
        {
            // Change the selection with the right hand primary2DAxis.x
            if (Mathf.Abs(InputManager.Input.primaryAxisR.x) > 0.05f && Mathf.Abs(InputManager.InputPrev.primaryAxisR.x) < 0.05f)
                ChangeActiveSelection((int) Mathf.Sign(InputManager.Input.primaryAxisR.x));

            if (InputManager.Input.primaryBtnR)
            {
                if (InputManager.Input.ActiveSelectionMode != SelectionMode.Toggle || !InputManager.InputPrev.primaryBtnR)
                {
                    Input.DoSelect = true;
                    Input.SelectPos = LibiglMesh.transform.InverseTransformPoint(InputManager.get.BrushR.center.position);
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
            Input.Shared = InputManager.Input;
            
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