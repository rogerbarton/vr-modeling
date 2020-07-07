using System;
using UnityEngine;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        /// <summary>
        /// The selections which are currently being translated by the left hand.
        /// This is set by the worker thread.
        /// </summary>
        private uint _currentTranslateMaskL;
        private uint _currentTranslateMaskR;

        /// <summary>
        /// Transforms the selections based on the <see cref="TransformDelta"/>s given in the <see cref="MeshInputState"/>
        /// It also decides which selections should be translated, storing this in <see cref="_currentTranslateMaskL"/>
        /// </summary>
        private void ActionTransformSelection()
        {
            if (_executeInput.Shared.ActiveTool != ToolType.Select ||
                !_executeInput.DoTransformL && !_executeInput.DoTransformR) return;

            // Find out which selections we should transform (via a local function)
            void CheckL()
            {
                if (!_executeInput.DoTransformL || _executeInput.DoTransformLPrev) return;

                // Set the mask as the active selection or the other hand's mask,
                // depending on if this hand is the primary one (beware of !)
                _currentTranslateMaskL = !_executeInput.PrimaryTransformHand
                    ? 1U << _executeInput.ActiveSelectionId
                    : _currentTranslateMaskR;
                FindBrushSelectionMask(ref _currentTranslateMaskL, _executeInput.BrushPosL);
            }

            void CheckR()
            {
                if (!_executeInput.DoTransformR || _executeInput.DoTransformRPrev) return;

                _currentTranslateMaskR = _executeInput.PrimaryTransformHand
                    ? 1U << _executeInput.ActiveSelectionId
                    : _currentTranslateMaskL;
                FindBrushSelectionMask(ref _currentTranslateMaskR, _executeInput.BrushPosR);
            }

            // Special case: when both LR pressed simultaneously => Check primary hand first
            if (_executeInput.PrimaryTransformHand)
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
            if (_executeInput.DoTransformL && !_executeInput.DoTransformR ||
                !_executeInput.DoTransformL && _executeInput.DoTransformR ||
                _currentTranslateMaskL == _currentTranslateMaskR)
            {
                ActionTransformSelectionGeneric(ref _executeInput.TransformDeltaJoint,
                    _executeInput.PrimaryTransformHand ? _currentTranslateMaskR : _currentTranslateMaskL);
            }
            else
            {
                if (_executeInput.DoTransformL)
                    ActionTransformSelectionGeneric(ref _executeInput.TransformDeltaL, _currentTranslateMaskL);
                if (_executeInput.DoTransformR)
                    ActionTransformSelectionGeneric(ref _executeInput.TransformDeltaR, _currentTranslateMaskR);
            }
        }

        /// <summary>
        /// Finds which selections are inside the brush and updates the <see cref="maskId"/>
        /// </summary>
        private void FindBrushSelectionMask(ref uint maskId, Vector3 brushPos)
        {
            // If selection inside brush is not zero then use that as the mask
            var brushMask = Native.GetSelectionMaskSphere(State, brushPos, _executeInput.Shared.BrushRadius);

            brushMask &= Input.VisibleSelectionMask;
            if (brushMask > 0)
                maskId = brushMask;
        }
        
        /// <summary>
        /// Does the actual translation, but is independent of the hands (L or R)
        /// </summary>
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

        /// <summary>
        /// Selects what is inside the brushes of the hands.
        /// </summary>
        private void ActionSelect()
        {
            ActionSelectGeneric(_executeInput.DoSelectL, _executeInput.BrushPosL, _executeInput.AlternateSelectModeL);
            ActionSelectGeneric(_executeInput.DoSelectR, _executeInput.BrushPosR, _executeInput.AlternateSelectModeR);
        }

        /// <summary>
        /// Does the actual selection, but is independent of the hands (L or R)
        /// </summary>
        private void ActionSelectGeneric(bool doSelect, Vector3 brushPos, bool alternateSelectMode)
        {
            var mode = _executeInput.Shared.ActiveSelectionMode;

            if (alternateSelectMode)
            {
                if (mode == SelectionMode.Add)
                    mode = SelectionMode.Subtract;
                else if (mode == SelectionMode.Subtract)
                    mode = SelectionMode.Add;
            }

            if (doSelect)
                Native.SelectSphere(State, brushPos, _executeInput.BrushRadiusLocal,
                    _executeInput.ActiveSelectionId, (uint) mode);
        }

        /// <summary>
        /// Runs the <c>igl::harmonic</c> Biharmonic Deformation 
        /// </summary>
        private void ActionHarmonic()
        {
            if (!_executeInput.DoHarmonic) return;

            Native.Harmonic(State, _executeInput.VisibleSelectionMask, _executeInput.HarmonicShowDisplacement);
        }

        /// <summary>
        /// Runs the <c>igl::arap</c> As-Rigid-As-Possible Deformation
        /// </summary>
        private void ActionArap()
        {
            if (!_executeInput.DoArap) return;

            Native.Arap(State, _executeInput.VisibleSelectionMask);
        }

        /// <summary>
        /// Applies various actions triggered from the UI or other input
        /// </summary>
        private void ActionUi()
        {
            if (_executeInput.DoClearSelection > 0)
                Native.ClearSelectionMask(State, _executeInput.DoClearSelection);

            if (_executeInput.VisibleSelectionMaskChanged)
                Native.SetColorByMask(State, _executeInput.VisibleSelectionMask);

            if (_executeInput.ResetV)
                Native.ResetV(State);
        }
    }
}