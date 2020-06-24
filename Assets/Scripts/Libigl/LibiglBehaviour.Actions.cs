using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        private uint _currentTranslateMask;
        private void ActionTransformSelection()
        {
            if (!ExecuteInput.DoTransform || ExecuteInput.Shared.ActiveTool != ToolType.Select) return;

            // Find out which selections we should transform
            if (ExecuteInput.DoTransform && !ExecuteInput.DoTransformPrev)
            {
                _currentTranslateMask = 1U << ExecuteInput.ActiveSelectionId;
                // If selection inside brush is not zero then use that as the mask
                var brushMask =
                    Native.GetSelectionMaskSphere(State, ExecuteInput.BrushPosR, ExecuteInput.Shared.BrushRadius);
                
                brushMask &= Input.VisibleSelectionMask;
                if (brushMask > 0)
                    _currentTranslateMask = brushMask;
            }

            if (ExecuteInput.TransformDelta.Rotate == Quaternion.identity && ExecuteInput.TransformDelta.Scale == 1f)
            {
                // Only translate selection
                Native.TranslateSelection(State, ExecuteInput.TransformDelta.Translate, _currentTranslateMask);
            }
            else
            {
                // Do full transformation
                Native.TransformSelection(State, ExecuteInput.TransformDelta.Translate,
                    ExecuteInput.TransformDelta.Scale, ExecuteInput.TransformDelta.Rotate, _currentTranslateMask);
            }
        }

        private void ActionSelect()
        {
            if (!ExecuteInput.DoSelect) return;
            
            Native.SelectSphere(State, ExecuteInput.BrushPosR, ExecuteInput.Shared.BrushRadius, 
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