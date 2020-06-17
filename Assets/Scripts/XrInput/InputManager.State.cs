using UnityEngine;
using UnityEngine.XR;

namespace XrInput
{
    public partial class InputManager
    {
        private void UpdateSharedState()
        {
            InputPrev = Input;

            LeftHand.TryGetFeatureValue(CommonUsages.primaryButton, out Input.primaryBtnL);
            LeftHand.TryGetFeatureValue(CommonUsages.primaryButton, out Input.secondaryBtnL);
            LeftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Input.primaryAxisL);
            
            RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out Input.primaryBtnR);
            RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out Input.secondaryBtnR);
            RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out Input.primaryAxisR);

            var xrRigRotation = XRRig.rotation;
            
            // Read values and then convert to world space
            LeftHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripL);
            LeftHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosL);
            LeftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotL);
            
            RightHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripR);
            RightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosR);
            RightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotR);

            // Convert to world space
            Input.HandPosL = XRRig.TransformPoint(Input.HandPosL);
            Input.HandRotL *= Quaternion.Inverse(xrRigRotation);
            Input.HandPosR = XRRig.TransformPoint(Input.HandPosR);
            Input.HandRotR *= Quaternion.Inverse(xrRigRotation);


            // Brush Resizing
            if (Mathf.Abs(Input.primaryAxisR.y) > 0.01f)
            {
                Input.BrushRadius = Mathf.Clamp(
                    Input.BrushRadius + XrBrush.ResizeSpeed * Input.primaryAxisR.y * Time.deltaTime,
                    XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                BrushL.SetRadius(Input.BrushRadius);
                BrushR.SetRadius(Input.BrushRadius);
            }

            
        }
    }
}