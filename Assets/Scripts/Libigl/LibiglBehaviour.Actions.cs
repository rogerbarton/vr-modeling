using System;
using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        private uint _currentTranslateMaskL;
        private uint _currentTranslateMaskR;

        private void ActionTransformSelection()
        {
            if (ExecuteInput.Shared.ActiveTool != ToolType.Select ||
                !ExecuteInput.DoTransformL && !ExecuteInput.DoTransformR) return;

            // Find out which selections we should transform (via a local function)
            void CheckL()
            {
                if (!ExecuteInput.DoTransformL || ExecuteInput.DoTransformLPrev) return;

                // Set the mask as the active selection or the other hand's mask,
                // depending on if this hand is the primary one (beware of !)
                _currentTranslateMaskL = !ExecuteInput.PrimaryTransformHand
                    ? 1U << ExecuteInput.ActiveSelectionId
                    : _currentTranslateMaskR;
                FindBrushSelectionMask(ref _currentTranslateMaskL, ExecuteInput.BrushPosL);
            }

            void CheckR()
            {
                if (!ExecuteInput.DoTransformR || ExecuteInput.DoTransformRPrev) return;

                _currentTranslateMaskR = ExecuteInput.PrimaryTransformHand
                    ? 1U << ExecuteInput.ActiveSelectionId
                    : _currentTranslateMaskL;
                FindBrushSelectionMask(ref _currentTranslateMaskR, ExecuteInput.BrushPosR);
            }

            // Special case: when both LR pressed simultaneously => Check primary hand first
            if (ExecuteInput.PrimaryTransformHand)
            {
                CheckR();
                CheckL();
            }
            else
            {
                CheckL();
                CheckR();
            }

            // Carry out the operation, if the masks are the same then do the joint operation
            if (ExecuteInput.DoTransformL && !ExecuteInput.DoTransformR ||
                !ExecuteInput.DoTransformL && ExecuteInput.DoTransformR ||
                _currentTranslateMaskL == _currentTranslateMaskR)
            {
                ActionTransformSelectionGeneric(ref ExecuteInput.TransformDeltaJoint,
                    ExecuteInput.PrimaryTransformHand ? _currentTranslateMaskR : _currentTranslateMaskL);
            }
            else
            {
                if (ExecuteInput.DoTransformL)
                    ActionTransformSelectionGeneric(ref ExecuteInput.TransformDeltaL, _currentTranslateMaskL);
                if (ExecuteInput.DoTransformR)
                    ActionTransformSelectionGeneric(ref ExecuteInput.TransformDeltaR, _currentTranslateMaskR);
            }
        }

        private void FindBrushSelectionMask(ref uint maskId, Vector3 brushPos)
        {
            // If selection inside brush is not zero then use that as the mask
            var brushMask = Native.GetSelectionMaskSphere(State, brushPos, ExecuteInput.Shared.BrushRadius);

            brushMask &= Input.VisibleSelectionMask;
            if (brushMask > 0)
                maskId = brushMask;
        }

        private void ActionTransformSelectionGeneric(ref TransformDelta transformDelta, uint maskId)
        {
            if (transformDelta.Rotate == Quaternion.identity && transformDelta.Scale == 1f)
            {
                // Only translate selection
                Native.TranslateSelection(State, transformDelta.Translate, maskId);
            }
            else
            {
                // Do full transformation
                // Ignore pivot mode and use hands as pivot
                // var doPivot = transformDelta.PivotMode != PivotMode.Mesh && 
                //               ExecuteInput.Shared.ToolTransformMode != ToolTransformMode.TransformingLR;
                var pivot = Vector3.zero;

                switch (transformDelta.PivotMode)
                {
                    case PivotMode.Mesh:
                    // Fall through, use hands as pivot
                    case PivotMode.Hand:
                        pivot = transformDelta.Pivot;
                        break;
                    case PivotMode.Selection:
                        pivot = Native.GetSelectionCenter(State, maskId);
                        break;
                }

                Native.TransformSelection(State, transformDelta.Translate,
                    transformDelta.Scale, transformDelta.Rotate, pivot, maskId);
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

            if (alternateSelectMode)
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

            if (ExecuteInput.VisibleSelectionMaskChanged)
                Native.SetColorByMask(State, ExecuteInput.VisibleSelectionMask);

            if (ExecuteInput.ResetV)
                Native.ResetV(State);
        }
    }
}