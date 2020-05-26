using UnityEngine;
using UnityEngine.XR;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        // C# only input variables
        private Vector2 _lastPrimaryAxisValueL;
        
        private void UpdateInput()
        {
            if (!InputManager.get.RightHand.isValid) return;

            switch (Input.ActiveTool)
            {
                case ToolType.Default:
                    UpdateInputDefault();
                    UpdateInputTransform();
                    break;
                case ToolType.Select:
                    UpdateInputSelect();
                    UpdateInputTransform();
                    break;
            }
        }

        /// <summary>
        /// Input for the default tool
        /// </summary>
        private void UpdateInputDefault()
        {
            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.secondaryButton,
                out var secondaryBtnValue) && secondaryBtnValue)
            {
                Input.DoTransform = true;
            }
        }

        /// <summary>
        /// Gathering input for the select tool
        /// </summary>
        private void UpdateInputSelect()
        {
            // Change the selection with the left hand primary2DAxis.y
            if (InputManager.get.LeftHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisL))
            {
                if (Mathf.Abs(_lastPrimaryAxisValueL.y) < 0.05f && Mathf.Abs(primaryAxisL.y) > 0.05f)
                    Input.ChangeActiveSelection((int) Mathf.Sign(primaryAxisL.y));

                _lastPrimaryAxisValueL = primaryAxisL;
            }

            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) &&
                primaryBtnValue)
            {
                Input.DoSelect = true;
                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.devicePosition,
                    out var rightHandPos))
                {
                    Input.SelectPos = LibiglMesh.transform.InverseTransformPoint(
                        InputManager.get.XRRig.TransformPoint(rightHandPos));
                }
                else
                    Debug.LogWarning("Could not get Right Hand Position");
            }

            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
            {
                if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                {
                    Input.SelectRadiusSqr = Mathf.Clamp(
                        Mathf.Sqrt(Input.SelectRadiusSqr) + 0.5f * primaryAxisValue.y * Time.deltaTime,
                        0.01f, 1f);
                    Input.SelectRadiusSqr *= Input.SelectRadiusSqr;
                }
            }
        }

        private void UpdateInputTransform()
        {
            HandTransformInput(InputManager.get.LeftHand, false, ref Input.GripL, ref Input.HandPosL);
            HandTransformInput(InputManager.get.RightHand, true, ref Input.GripR, ref Input.HandPosR);
        }

        /// <summary>
        /// Updates transform tool input for a hand
        /// </summary>
        /// <param name="inputGrip">Where to store the trigger input value</param>
        /// <param name="inputHandPos">Where to store the hand position input value</param>
        private void HandTransformInput(InputDevice inputDevice, bool isRight, ref float inputGrip, ref Vector3 inputHandPos)
        {
            if (!inputDevice.TryGetFeatureValue(CommonUsages.grip, out var grip)) return;
            
            inputGrip = grip;
            inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out var handPos);
            inputHandPos = handPos;

            // Handling changes in the selection 'state machine'
            if (grip > 0.01f)
            {
                if (!Input.DoTransform)
                {
                    Input.DoTransform = true;
                    Input.PrimaryTransformHand = isRight;
                }
                else
                    Input.SecondaryTransformHandActive = true;
            }
            else
            {
                if (Input.DoTransform && Input.PrimaryTransformHand)
                {
                    if (Input.SecondaryTransformHandActive)
                        Input.PrimaryTransformHand = !isRight;
                    else
                        Input.DoTransform = false;
                }
            }

        }

        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            if(Input.ActiveTool == ToolType.Select && Input.DoTransform)
            {
                // Only update this if we are transforming on the thread, i.e. transforming the selection
                Input.PrevTrafoHandPosL = Input.HandPosL;
                Input.PrevTrafoHandPosR = Input.HandPosR;
            }
            
            // Consume inputs here
            Input.DoTransform = false;
            Input.DoSelect = false;
            Input.DoHarmonic = false;
        }
    }
}