using UnityEngine;
using UnityEngine.XR;

namespace XrInput
{
    public partial class InputManager
    {
        private void UpdateSharedState()
        {
            LeftHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripL);
            LeftHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosL);
            RightHand.TryGetFeatureValue(CommonUsages.grip, out Input.GripR);
            RightHand.TryGetFeatureValue(CommonUsages.devicePosition, out Input.HandPosR);
        }
    }
}