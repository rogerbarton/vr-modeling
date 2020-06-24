using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace XrInput
{
    public partial class InputManager
    {
        /// <summary>
        /// Updates the <see cref="SharedInputState"/> <see cref="State"/>
        /// </summary>
        private void UpdateSharedState()
        {
            StatePrev = State;

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.SecondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.PrimaryAxisL);
            
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.PrimaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.SecondaryBtnR);
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

            // Input conflict with interactables
            State.GripR *= handInteractorR.selectTarget ? 0f : 1f;

            var hoverTargets = new List<XRBaseInteractable>();
            handInteractorL.GetHoverTargets(hoverTargets);
            State.TriggerL *= hoverTargets.Count == 0 ? 1f : 0f;
            handInteractorR.GetHoverTargets(hoverTargets);
            State.TriggerR *= hoverTargets.Count == 0 ? 1f : 0f;

            // Brush Resizing
            if (Mathf.Abs(State.PrimaryAxisR.y) > 0.01f)
            {
                State.BrushRadius = Mathf.Clamp(
                    State.BrushRadius + XrBrush.ResizeSpeed * State.PrimaryAxisR.y * Time.deltaTime,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                BrushL.SetRadius(State.BrushRadius);
                BrushR.SetRadius(State.BrushRadius);
            }

            if (State.ActiveTool == ToolType.Transform &&
                State.TriggerR > 0.1f && StatePrev.TriggerR < 0.1f)
            {
                BrushR.SetActiveMesh();
            }
        }
    }
}