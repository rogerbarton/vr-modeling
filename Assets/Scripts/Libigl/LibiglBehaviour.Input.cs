using UnityEngine;
using UnityEngine.XR;
using XrInput;

namespace Libigl
{
    public unsafe partial class LibiglBehaviour
    {
        // C# only input variables
        private Vector2 _lastPrimaryAxisValueL;
        
        private void UpdateInput()
        {
            if (!InputManager.get.RightHand.isValid) return;

            switch (InputManager.Input.ActiveTool)
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
                    Input.SelectPos = LibiglMesh.transform.InverseTransformPoint(InputManager.get.BrushR.center.position);
                }
                else
                    Debug.LogWarning("Could not get Right Hand Position");
            }

            if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
            {
                if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                {
                    var brush = InputManager.get.BrushR;
                    Input.SelectRadius = Mathf.Clamp(Input.SelectRadius + 0.5f * primaryAxisValue.y * Time.deltaTime,
                        brush.RadiusRange.x, brush.RadiusRange.y);

                    brush.SetRadius(Input.SelectRadius);
                }
            }
        }

        private void UpdateInputTransform()
        {
            MapTransformActions(InputManager.get.LeftHand, false, InputManager.Input.GripL);
            MapTransformActions(InputManager.get.RightHand, true, InputManager.Input.GripL);
        }

        /// <summary>
        /// Updates transform tool input for a hand
        /// </summary>
        private void MapTransformActions(InputDevice inputDevice, bool isRight, float grip)
        {
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
                 if(Input.PrimaryTransformHand == isRight)
                 {
                     if (Input.SecondaryTransformHandActive)
                         Input.PrimaryTransformHand = !isRight;
                     else
                         Input.DoTransform = false;
                 }
                 else
                     Input.SecondaryTransformHandActive = false;
            }
        }

        private void PreExecuteInput()
        {
            Input.SharedPrev = Input.Shared;
            Input.Shared = InputManager.Input;
            
            Input.DoTransform |= UnityEngine.Input.GetKeyDown(KeyCode.W);
            Input.DoSelect |= UnityEngine.Input.GetMouseButtonDown(0);
        }
        
        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            Input.DoTransformPrev = Input.DoTransform;
            Input.DoTransform = false;
            Input.DoSelectPrev = Input.DoSelect;
            Input.DoSelect = false;
            Input.DoClearSelection = 0;
            Input.VisibleSelectionMaskChanged = false;
            if(!Input.DoHarmonicRepeat)
                Input.DoHarmonic = false;
            if(!Input.DoArapRepeat)
                Input.DoArap = false;
            Input.ResetV = false;
        }
    }
}