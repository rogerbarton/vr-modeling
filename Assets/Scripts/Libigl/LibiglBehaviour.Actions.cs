using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransformSelection()
        {
            if (!ExecuteInput.DoTransform || !ExecuteInput.DoTransformPrev || ExecuteInput.Shared.ActiveTool != ToolType.Select) return;

            if (ExecuteInput.RotateDelta == Quaternion.identity && ExecuteInput.ScaleDelta == 1f)
            {
                // Only translate selection
                Native.TranslateSelection(State, ExecuteInput.TranslateDelta, ExecuteInput.ActiveSelectionId);
            }
            else
            {
                // Do full transformation
                GetTransformData(ref ExecuteInput, out var translate, out var scale, out var angle, out var axis);
                angle *= Mathf.Deg2Rad; // Eigen uses rad, Unity uses deg

                Native.TransformSelection(State, ExecuteInput.ActiveSelectionId, ExecuteInput.TranslateDelta,
                    ExecuteInput.ScaleDelta, ExecuteInput.RotateDelta);
            }
        }

        private void ActionSelect()
        {
            if (!ExecuteInput.DoSelect) return;
            
            Native.SelectSphere(State, ExecuteInput.SelectPos, ExecuteInput.Shared.BrushRadius, 
                ExecuteInput.ActiveSelectionId, (uint) ExecuteInput.Shared.ActiveSelectionMode);
        }

        private void ActionHarmonic()
        {
            if (!ExecuteInput.DoHarmonic) return;
            
            Native.Harmonic(State, ExecuteInput.VisibleSelectionMask, ExecuteInput.HarmonicShowDisplacement);
        }
        
        private void ActionArap()
        {
            if (!ExecuteInput.DoArap) return;
            
            Native.Arap(State, ExecuteInput.VisibleSelectionMask);
        }

        private void ActionUi()
        {
            if (ExecuteInput.DoClearSelection > 0)
                Native.ClearSelectionMask(State, ExecuteInput.DoClearSelection);
            
            if(ExecuteInput.VisibleSelectionMaskChanged)
                Native.SetColorByMask(State, ExecuteInput.VisibleSelectionMask);

            if (ExecuteInput.ResetV)
                Native.ResetV(State);
        }

        private void UpdateMeshTransform()
        {
            if (InputManager.Input.ActiveTool == ToolType.Select || !Input.DoTransform || !Input.DoTransformPrev) return;
            
            // Transform the whole mesh
            if (Input.SecondaryTransformHandActive)
            {
                //TODO: Use InputManager.InputPrev
                GetTransformData(ref Input, out var translate, out var scale, out var angle, out var axis);
                var uTransform = LibiglMesh.transform;
                uTransform.Translate(translate);
                uTransform.Rotate(axis, angle);
                uTransform.localScale *= scale;
            }
            else
                LibiglMesh.transform.Translate(GetTranslateVector(ref Input));
        }
    }
}