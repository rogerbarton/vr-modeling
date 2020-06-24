using System;
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

            if (ExecuteInput.TransformDeltaJoint.Rotate == Quaternion.identity && ExecuteInput.TransformDeltaJoint.Scale == 1f)
            {
                // Only translate selection
                Native.TranslateSelection(State, ExecuteInput.TransformDeltaJoint.Translate, _currentTranslateMaskP);
            }
            else
            {
                // Do full transformation
                // Ignore pivot mode and use hands as pivot
                // var doPivot = ExecuteInput.TransformDelta.PivotMode != PivotMode.Mesh && 
                //               ExecuteInput.Shared.ToolTransformMode != ToolTransformMode.TransformingLR;
                var pivot = Vector3.zero;
                
                switch (ExecuteInput.TransformDeltaJoint.PivotMode)
                {
                    case PivotMode.Mesh:
                        // Fall through, use hands as pivot
                    case PivotMode.Hand:
                        pivot = ExecuteInput.TransformDeltaJoint.Pivot;
                        break;
                    case PivotMode.Selection:
                        pivot = Native.GetSelectionCenter(State, _currentTranslateMask);
                        break;
                }

                Native.TransformSelection(State, ExecuteInput.TransformDeltaJoint.Translate,
                    ExecuteInput.TransformDeltaJoint.Scale, ExecuteInput.TransformDeltaJoint.Rotate, pivot,
                    _currentTranslateMask);
            }
        }

        private void ActionSelect()
        {
            ActionSelectGeneric(ExecuteInput.DoSelectL, ExecuteInput.BrushPosL, ExecuteInput.AlternateSelectModeL);
            ActionSelectGeneric(ExecuteInput.DoSelectR, ExecuteInput.BrushPosR, ExecuteInput.AlternateSelectModeR);
        }

        private void ActionSelectGeneric(bool doSelect, Vector3 brushPos, bool alternateSelectMode)
        {
            var mode = ExecuteInput.Shared.ActiveSelectionMode;

            if(alternateSelectMode)
            {
                if (mode == SelectionMode.Add)
                    mode = SelectionMode.Subtract;
                else if (mode == SelectionMode.Subtract)
                    mode = SelectionMode.Add;
            }
            
            if (doSelect)
                Native.SelectSphere(State, brushPos, ExecuteInput.Shared.BrushRadius,
                    ExecuteInput.ActiveSelectionId, (uint) mode);
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