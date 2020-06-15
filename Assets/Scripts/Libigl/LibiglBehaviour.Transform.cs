using System;
using UnityEngine;
using UnityEngine.XR;
using XrInput;

namespace Libigl
{
    public partial class LibiglBehaviour
    {
        private bool _primaryTransformHand; // True=R
        private bool _secondaryTransformHandActive;
        private Vector3 _transformStartHandPosL;
        private Vector3 _transformStartHandPosR;
        private Vector3 _transformStartHandRotL;
        private Vector3 _transformStartHandRotR;
        
        private void ResetInputTransform()
        {
            Input.TranslateDelta = Vector3.zero;
            Input.ScaleDelta = 1f;
            Input.RotateDelta = Quaternion.identity;
        }

        private void UpdateTransform()
        {
            // Call this in Update() every frame
            // Only handle events when the input has changed
            
            // on pressed
                // Set start pos/rot for appropriate hand
                
            // on depressed
                // AddTransformDelta
        }

        // Only for selections, transforming the mesh is done directly
        private void AddTransformDelta()
        {
            // Find out which mode we are in, call the according function
            switch (InputManager.Input.ActiveTransformMode)
            {
                case TransformMode.OneHandedTranslate:
                    TransformOneHandedTranslate();
                    break;
                case TransformMode.OneHandedTranslateRotate:
                    TransformOneHandedTranslateRotate();
                    break;
                case TransformMode.TwoHandedIndividualTranslate:
                    break;
                case TransformMode.TwoHandedIndividualTranslateRotate:
                    break;
                case TransformMode.TwoHandedJoint:
                    break;
                case TransformMode.TwoHandedJointScale:
                    break;
            }
        }
        
        // 1. One Handed Translate
        private void TransformOneHandedTranslate()
        {
            Vector3 t;
            if (_primaryTransformHand)
                t = Input.Shared.HandPosR - Input.SharedPrev.HandPosR;
            else
                t = Input.Shared.HandPosL - Input.SharedPrev.HandPosL;

            var softFactor = _primaryTransformHand ? Input.Shared.GripR : Input.Shared.GripL;
            Input.TranslateDelta += t * softFactor;
        }

        // 2. One Handed Translate+Rotate
        private void TransformOneHandedTranslateRotate()
        {
            Vector3 t;
            if (_primaryTransformHand)
                t = Input.Shared.HandPosR - Input.SharedPrev.HandPosR;
            else
                t = Input.Shared.HandPosL - Input.SharedPrev.HandPosL;

            var softFactor = _primaryTransformHand ? Input.Shared.GripR : Input.Shared.GripL;
            Input.TranslateDelta += t * softFactor;
        }












        // OLDER CODE - BEWARE!
        
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
                    _primaryTransformHand = isRight;
                }
                else
                    _secondaryTransformHandActive = true;
            }
            else
            {
                if(_primaryTransformHand == isRight)
                {
                    if (_secondaryTransformHandActive)
                        _primaryTransformHand = !isRight;
                    else
                        Input.DoTransform = false;
                }
                else
                    _secondaryTransformHandActive = false;
            }
        }
        
        /// <summary>
        /// Determines the softenes translation vector
        /// </summary>
        /// <param name="i">The input state to use</param>
        /// <returns></returns>
        private static Vector3 GetTranslateVector(ref InputState i)
        {
            Vector3 t;
            if(i.PrimaryTransformHand)
                t = i.Shared.HandPosR - i.SharedPrev.HandPosR;
            else
                t = i.Shared.HandPosL - i.SharedPrev.HandPosL;
            
            var softFactor = i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;
            return t * softFactor;
        }
        
        /// <summary>
        /// Determines the softened transformation from the input state
        /// </summary>
        /// <param name="i">InputState to use</param>
        /// <param name="angle">In degrees</param>
        private static void GetTransformData(ref InputState i, out Vector3 translate, out float scale, out float angle, out Vector3 axis)
        {
            Vector3 v0, v1;
            if(i.PrimaryTransformHand)
            {
                translate = i.Shared.HandPosR - i.SharedPrev.HandPosR;
                v0 = i.SharedPrev.HandPosR - i.SharedPrev.HandPosL;
                v1 = i.Shared.HandPosR - i.Shared.HandPosL;
            }
            else
            {
                translate = i.Shared.HandPosL - i.SharedPrev.HandPosL;
                v0 = i.SharedPrev.HandPosL - i.SharedPrev.HandPosR;
                v1 = i.Shared.HandPosL - i.Shared.HandPosR;
            }
                
            axis = Vector3.Cross(v0, v1);
            angle = Vector3.Angle(v0, v1);
            
            // TODO: scale should be done from positions at start of both grips pressed 
            scale = (i.Shared.HandPosL - i.Shared.HandPosR).magnitude / (i.SharedPrev.HandPosL - i.SharedPrev.HandPosR).magnitude;
            if (float.IsNaN(scale))
                scale = 1f;
            // Apply soft editing
            var softFactor = i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;
            var softFactorSecondary = !i.PrimaryTransformHand ? i.Shared.GripR : i.Shared.GripL;

            translate *= softFactor;
            scale = (scale -1) * softFactorSecondary + 1;
            angle *= softFactorSecondary;
        }
    }
}