using UnityEngine;
using UnityEngine.XR;

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

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.primaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out State.secondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.primaryAxisL);
            
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.primaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out State.secondaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primary2DAxis, out State.primaryAxisR);

            var xrRigRotation = XRRig.rotation;
            
            // Read values and then convert to world space
            HandL.TryGetFeatureValue(CommonUsages.grip, out State.GripL);
            HandL.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosL);
            HandL.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotL);
            
            HandR.TryGetFeatureValue(CommonUsages.grip, out State.GripR);
            HandR.TryGetFeatureValue(CommonUsages.devicePosition, out State.HandPosR);
            HandR.TryGetFeatureValue(CommonUsages.deviceRotation, out State.HandRotR);

            // Convert to world space
            State.HandPosL = XRRig.TransformPoint(State.HandPosL);
            State.HandRotL *= Quaternion.Inverse(xrRigRotation);
            State.HandPosR = XRRig.TransformPoint(State.HandPosR);
            State.HandRotR *= Quaternion.Inverse(xrRigRotation);


            // Brush Resizing
            if (Mathf.Abs(State.primaryAxisR.y) > 0.01f)
            {
                State.BrushRadius = Mathf.Clamp(
                    State.BrushRadius + XrBrush.ResizeSpeed * State.primaryAxisR.y * Time.deltaTime,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                BrushL.SetRadius(State.BrushRadius);
                BrushR.SetRadius(State.BrushRadius);
            }

            
        }
    }
}