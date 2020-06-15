using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransformSelection()
        {
            if (!ExecuteInput.DoTransform || !ExecuteInput.DoTransformPrev || ExecuteInput.Shared.ActiveTool != ToolType.Select) return;

            if (ExecuteInput.TransformDelta.Rotate == Quaternion.identity && ExecuteInput.TransformDelta.Scale == 1f)
            {
                // Only translate selection
                Native.TranslateSelection(State, ExecuteInput.TransformDelta.Translate, ExecuteInput.ActiveSelectionId);
            }
            else
            {
                // Do full transformation
                Native.TransformSelection(State, ExecuteInput.ActiveSelectionId, ExecuteInput.TransformDelta.Translate,
                    ExecuteInput.TransformDelta.Scale, ExecuteInput.TransformDelta.Rotate);
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
    }
}