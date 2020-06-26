using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace XrInput
{
    public partial class InputManager
    {
        private bool _isTeleporting;
        private bool _isTeleportingPrev;
        private readonly List<XRBaseInteractable> _hoverTargetsL = new List<XRBaseInteractable>();
        private readonly List<XRBaseInteractable> _hoverTargetsR = new List<XRBaseInteractable>();

        /// <summary>
        /// Updates the <see cref="SharedInputState"/> <see cref="State"/>
        /// </summary>
        private void UpdateSharedState()
        {
            StatePrev = State;

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisL);

            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.secondaryButton, out State.SecondaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisR);


            // Read values and then convert to world space
            HandL.TryGetFeatureValue(CommonUsages.grip, out State.GripL);
            HandL.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerL);
            HandL.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosL);
            HandL.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotL);

            HandR.TryGetFeatureValue(CommonUsages.grip, out State.GripR);
            HandR.TryGetFeatureValue(CommonUsages.trigger, out State.TriggerR);
            HandR.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosR);
            HandR.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotR);

            // Convert to world space
            var xrRigRotation = xrRig.rotation;
            State.HandPosL = xrRig.TransformPoint(State.HandPosL);
            State.HandRotL = xrRigRotation * State.HandRotL;
            State.HandPosR = xrRig.TransformPoint(State.HandPosR);
            State.HandRotR = xrRigRotation * State.HandRotR;


            // Update hover targets of rays
            _isTeleportingPrev = _isTeleporting;
            _isTeleporting = State.PrimaryAxisL.y > 0.1f; // Threshold should be lower than teleportRayL's
            if (!_isTeleporting)
            {
                if (handInteractorL)
                    handInteractorL.GetHoverTargets(_hoverTargetsL);
                if (handInteractorR)
                    handInteractorR.GetHoverTargets(_hoverTargetsR);
            }

            // Ray Interactor & Teleportation
            UpdateRayInteractors();

            // Input conflict with interactables
            if (handInteractorL)
                State.GripL *= handInteractorL.selectTarget ? 0f : 1f;
            if (handInteractorR)
                State.GripR *= handInteractorR.selectTarget ? 0f : 1f;
            State.TriggerL *= !_isTeleporting && _hoverTargetsL.Count == 0 ? 1f : 0f;
            State.TriggerR *= !_isTeleporting && _hoverTargetsR.Count == 0 ? 1f : 0f;

            // Changing Active Tool
            if (State.SecondaryBtnL && !StatePrev.SecondaryBtnL)
                SetActiveTool((ToolType) 
                    ((State.ActiveTool.GetHashCode() + 1) % Enum.GetNames(typeof(ToolType)).Length));

            // Brush Resizing
            if (Mathf.Abs(State.PrimaryAxisR.y) > 0.01f)
            {
                State.BrushRadius = Mathf.Clamp(
                    State.BrushRadius + XrBrush.ResizeSpeed * State.PrimaryAxisR.y * Time.deltaTime,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                if (BrushL)
                    BrushL.SetRadius(State.BrushRadius);
                BrushR.SetRadius(State.BrushRadius);
            }

            // Changing the Active Mesh
            if (State.ActiveTool == ToolType.Transform &&
                State.TriggerR > 0.1f && StatePrev.TriggerR < 0.1f)
            {
                BrushR.SetActiveMesh();
            }

        }

        private void UpdateRayInteractors()
        {
            if (!handInteractorL) return;

            if (_isTeleporting && !_isTeleportingPrev) // on pressed
            {
                teleportRayL.gameObject.SetActive(true);
                handInteractorL.gameObject.SetActive(false);
                if (handInteractorR)
                    handInteractorR.gameObject.SetActive(false);
            }
            else if (!_isTeleporting && _isTeleportingPrev) // on depressed
            {
                teleportRayL.gameObject.SetActive(false);
                handInteractorL.gameObject.SetActive(true);
                if (handInteractorR)
                    handInteractorR.gameObject.SetActive(true);
            }
            else if (!_isTeleporting)
            {
                // Only allow UI interaction if we are idle, not grabbing anything and hovering over a UI object
                var allowUiInteraction =
                    (State.ActiveTool == ToolType.Transform && State.ToolTransformMode == ToolTransformMode.Idle
                     || State.ActiveTool == ToolType.Select && State.ToolSelectMode == ToolSelectMode.Idle);

                handInteractorL.enableUIInteraction =
                    allowUiInteraction &&
                    (!handInteractorL.selectTarget ||
                     handInteractorL.selectTarget.gameObject.layer == UiManager.get.UiLayer) &&
                    _hoverTargetsL.Exists(t => t.gameObject.layer == UiManager.get.UiLayer);

                if (handInteractorR)
                    handInteractorR.enableUIInteraction =
                        allowUiInteraction &&
                        (!handInteractorR.selectTarget ||
                         handInteractorR.selectTarget.gameObject.layer == UiManager.get.UiLayer) &&
                        _hoverTargetsR.Exists(t => t.gameObject.layer == UiManager.get.UiLayer);
            }
        }
    }
}