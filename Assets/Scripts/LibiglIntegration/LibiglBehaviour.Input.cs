using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void UpdateInput()
        {
            if (InputManager.get.RightHand.isValid)
            {
                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.secondaryButton,
                    out var secondaryBtnValue) && secondaryBtnValue)
                {
                    _input.Translate = true;
                }


                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) &&
                    primaryBtnValue)
                {
                    _input.Select = true;
                    if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.devicePosition, 
                        out var rightHandPos))
                    {
                        _input.SelectPos = _libiglMesh.transform.InverseTransformPoint(
                                InputManager.get.XRRig.TransformPoint(rightHandPos));
                    }
                    else 
                        Debug.LogWarning("Could not get Right Hand Position");
                }

                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
                {
                    if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                    {
                        _input.SelectRadiusSqr = Mathf.Clamp(Mathf.Sqrt(_input.SelectRadiusSqr) + 0.5f * primaryAxisValue.y * Time.deltaTime, 
                            0.01f, 1f);
                        _input.SelectRadiusSqr *= _input.SelectRadiusSqr;
                    }
                }
            }
        }

        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            _input.Translate = false;
            _input.Select = false;
            _input.Harmonic = false;
        }
    }
}