using System;
using UnityEngine;
using UnityEngine.XR;
using XrInput;

namespace Libigl
{
    public partial class LibiglBehaviour
    {
        private bool _primaryTransformHand; // True=R
        private bool _doTransformL;
        private bool _doTransformR;
        private Vector3 _transformStartHandPosL;
        private Vector3 _transformStartHandPosR;
        private Quaternion _transformStartHandRotL;
        private Quaternion _transformStartHandRotR;
        
        private void ConsumeInputTransform()
        {
            Input.TranslateDelta = Vector3.zero;
            Input.ScaleDelta = 1f;
            Input.RotateDelta = Quaternion.identity;

            ResetTransformStartPositions();
        }

        private void ResetTransformStartPositions()
        {
            _transformStartHandPosL = InputManager.Input.HandPosL;
            _transformStartHandRotL = InputManager.Input.HandRotL;
            _transformStartHandPosR = InputManager.Input.HandPosR;
            _transformStartHandRotR = InputManager.Input.HandRotR;
        }

        private const float PressThres = 0.1f;
        private void UpdateTransformState()
        {
            // Call this in Update() every frame
            // Only handle events when the input has changed
            
            // on pressed
            if (InputManager.Input.GripL > PressThres && InputManager.InputPrev.GripL < PressThres)
            {
                // Set start pos/rot for appropriate hand
                _transformStartHandPosL = InputManager.Input.HandPosL;
                _transformStartHandRotL = InputManager.Input.HandRotL;
                _doTransformL = true;
                
                if (!Input.DoTransform)
                {
                    Input.DoTransform = true;
                    _primaryTransformHand = false;
                }
            }
            
            
            // while pressing
            // TransformMesh

            
            // on depressed
            if (InputManager.Input.GripL < PressThres && InputManager.InputPrev.GripL > PressThres)
            {
                _doTransformL = true;
                
                // GetTransformDelta
                GetTransformDelta();
                
                // Change state
                if(_primaryTransformHand == false)
                {
                    if (_doTransformR)
                        _primaryTransformHand = true;
                    else
                        Input.DoTransform = false;
                }
            }
        }

        // Only for selections, transforming the mesh is done directly
        private void GetTransformDelta()
        {
            // Find out which mode we are in, call the according function
            switch (InputManager.Input.ActiveTransformMode)
            {
                case TransformMode.IndividualTranslate:
                    TransformIndividual(false);
                    break;
                case TransformMode.IndividualTranslateRotate:
                    TransformIndividual(true);
                    break;
                case TransformMode.Joint:
                    TransformJoint(false);
                    break;
                case TransformMode.JointScale:
                    TransformJoint(true);
                    break;
            }

            ResetTransformStartPositions();
        }



        private void TransformIndividual(bool withRotate)
        {
            Vector3 translate;
            if (_primaryTransformHand)
                translate = InputManager.Input.HandPosR - _transformStartHandPosR;
            else
                translate = InputManager.Input.HandPosL - _transformStartHandPosL;

            var softFactor = _primaryTransformHand ? InputManager.Input.GripR : InputManager.Input.GripL;
            Input.TranslateDelta += translate * softFactor;

            
            // Rotate - Optional
            if (!withRotate) return;

            Quaternion rotate;
            if (_primaryTransformHand)
                rotate = InputManager.Input.HandRotR * Quaternion.Inverse(_transformStartHandRotR);
            else
                rotate = InputManager.Input.HandRotR * Quaternion.Inverse(_transformStartHandRotR);

            Input.RotateDelta *= Quaternion.Lerp(Quaternion.identity, rotate, softFactor);
        }

        private void TransformJoint(bool withScale)
        {
            Vector3 v0, v1, translate;
            if(_primaryTransformHand)
            {
                translate = InputManager.Input.HandPosR - Input.SharedPrev.HandPosR;
                v0 = Input.SharedPrev.HandPosR - Input.SharedPrev.HandPosL;
                v1 = InputManager.Input.HandPosR - InputManager.Input.HandPosL;
            }
            else
            {
                translate = InputManager.Input.HandPosL - Input.SharedPrev.HandPosL;
                v0 = Input.SharedPrev.HandPosL - Input.SharedPrev.HandPosR;
                v1 = InputManager.Input.HandPosL - InputManager.Input.HandPosR;
            }
                
            var axis = Vector3.Cross(v0, v1);
            var angle = Vector3.Angle(v0, v1);

            // Apply soft editing
            var softFactor = _primaryTransformHand ? InputManager.Input.GripR : InputManager.Input.GripL;
            var softFactorSecondary = !_primaryTransformHand ? InputManager.Input.GripR : InputManager.Input.GripL;

            translate *= softFactor;
            angle *= softFactorSecondary;

            // Apply to InputState
            Input.TranslateDelta += translate;
            Input.RotateDelta *= Quaternion.AngleAxis(angle, axis);

            
            // Scale - Optional
            if (!withScale) return;
            
            var scale = (InputManager.Input.HandPosL - InputManager.Input.HandPosR).magnitude / 
                        (_transformStartHandPosL - _transformStartHandPosR).magnitude;
            if (float.IsNaN(scale))
                scale = 1f;
                
            scale = (scale -1) * softFactorSecondary + 1;

            Input.ScaleDelta *= scale;
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