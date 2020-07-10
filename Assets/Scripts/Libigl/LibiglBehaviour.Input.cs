using System;
using UnityEngine;
using XrInput;

namespace Libigl
{
    public partial class LibiglBehaviour
    {
        /// <summary>
        /// Updates the <see cref="Input"/> every frame, from Update().
        /// </summary>
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

            if (InputManager.get.BrushL)
                Input.BrushPosL = Mesh.transform.InverseTransformPoint(InputManager.get.BrushL.center.position);
            if (InputManager.get.BrushR)
                Input.BrushPosR = Mesh.transform.InverseTransformPoint(InputManager.get.BrushR.center.position);
        }

        /// <summary>
        /// Input for the transform tool
        /// </summary>
        private void UpdateInputTransform()
        {
            // Change the transform tool mode
            if (_doTransformL)
                InputManager.State.ToolTransformMode =
                    _doTransformR ? ToolTransformMode.TransformingLr : ToolTransformMode.TransformingL;
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
                InputManager.State.ToolSelectMode =
                    _doTransformR ? ToolSelectMode.TransformingLr : ToolSelectMode.TransformingL;
            else if (_doTransformR)
                InputManager.State.ToolSelectMode = ToolSelectMode.TransformingR;
            else if (InputManager.State.TriggerL > GrabPressThreshold || InputManager.State.TriggerR > GrabPressThreshold)
            {
                InputManager.State.ToolSelectMode = ToolSelectMode.Selecting;

                if (InputManager.State.TriggerL > GrabPressThreshold &&
                    (InputManager.State.ActiveSelectionMode != SelectionMode.Invert ||
                     InputManager.StatePrev.TriggerL < GrabPressThreshold)
                    && InputManager.get.BrushL.InsideActiveMeshBounds)
                    Input.DoSelectL = true;

                if (InputManager.State.TriggerR > GrabPressThreshold &&
                    (InputManager.State.ActiveSelectionMode != SelectionMode.Invert ||
                     InputManager.StatePrev.TriggerR < GrabPressThreshold)
                    && InputManager.get.BrushR.InsideActiveMeshBounds)
                    Input.DoSelectR = true;

                Input.AlternateSelectModeL = InputManager.State.PrimaryBtnL;
                Input.AlternateSelectModeR = InputManager.State.PrimaryBtnR;
            }
            else
            {
                InputManager.State.ToolSelectMode = ToolSelectMode.Idle;

                if (InputManager.State.SecondaryBtnR && !InputManager.StatePrev.SecondaryBtnR)
                    _uiDetails.AddSelection();
            }

            // Change the selection with the right hand primary2DAxis.x
            if (Mathf.Abs(InputManager.State.PrimaryAxisR.x) > 0.4f &&
                Mathf.Abs(InputManager.StatePrev.PrimaryAxisR.x) < 0.4f)
                SetActiveSelectionIncrement((int) Mathf.Sign(InputManager.State.PrimaryAxisR.x));
        }

        /// <summary>
        /// Updates the <see cref="Input"/> just before the worker thread is started.
        /// This copies the shared <see cref="InputManager.State"/> to the <see cref="Input"/>
        /// </summary>
        private void PreExecuteInput()
        {
            Input.SharedPrev = Input.Shared;
            Input.Shared = InputManager.State;

            Input.BrushRadiusLocal = InputManager.State.BrushRadius / Mesh.transform.localScale.magnitude;
        }

        /// <summary>
        /// Invoked when the active selection of the mesh has changed.
        /// </summary>
        public event Action OnActiveSelectionChanged = delegate { };

        /// <summary>
        /// Changes the active selection and triggers <see cref="OnActiveSelectionChanged"/>.
        /// </summary>
        public void SetActiveSelection(int value)
        {
            if (Input.ActiveSelectionId == value) return;

            Input.ActiveSelectionId = value;
            OnActiveSelectionChanged();
        }

        /// <summary>
        /// Increments the active selection and safely loops.
        /// </summary>
        public void SetActiveSelectionIncrement(int increment)
        {
            SetActiveSelection((int) ((Input.ActiveSelectionId + increment + Input.SCountUi) % Input.SCountUi));
        }
    }
}