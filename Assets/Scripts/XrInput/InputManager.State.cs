using UnityEngine;
using UnityEngine.XR;

namespace XrInput
{
    public partial class InputManager
    {
        private void UpdateSharedState()
        {
            InputPrev = Input;

            LeftHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripL);
            LeftHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosL);
            LeftHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotL);
            
            RightHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripR);
            RightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosR);
            RightHand.TryGetFeatureValue(CommonUsages.deviceRotation, out Input.HandRotR);

            // Brush Resizing
            if (RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
            {
                if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                {
                    Input.BrushRadius = Mathf.Clamp(
                        Input.BrushRadius + XrBrush.ResizeSpeed * primaryAxisValue.y * Time.deltaTime,
                        XrBrush.RadiusRange.x, XrBrush.RadiusRange.y);

                    BrushL.SetRadius(Input.BrushRadius);
                    BrushR.SetRadius(Input.BrushRadius);
                }
            }
        }
    }
}