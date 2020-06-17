using UnityEngine;
using UnityEngine.XR;

namespace XrInput
{
    public partial class InputManager
    {
        private void UpdateSharedState()
        {
            InputPrev = Input;

            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out Input.primaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primaryButton, out Input.secondaryBtnL);
            HandL.TryGetFeatureValue(CommonUsages.primary2DAxis, out Input.primaryAxisL);
            
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out Input.primaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primaryButton, out Input.secondaryBtnR);
            HandR.TryGetFeatureValue(CommonUsages.primary2DAxis, out Input.primaryAxisR);

            var xrRigRotation = XRRig.rotation;
            
            // Read values and then convert to world space
            HandL.TryGetFeatureValue(CommonUsages.grip, out Input.GripL);
            HandL.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosL);
            HandL.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotL);
            
            HandR.TryGetFeatureValue(CommonUsages.grip, out Input.GripR);
            HandR.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosR);
            HandR.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotR);

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