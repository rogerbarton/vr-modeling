using System;
using UnityEngine;
using UnityEngine.XR;
using XrInput;

namespace Libigl
{
    public struct TransformDelta
    {
        public Vector3 Translate;
        public Quaternion Rotate;
        public float Scale;

        public static TransformDelta Identity()
        {
            return new TransformDelta {Rotate = Quaternion.identity, Scale = 1f};
        }

        public void Add(TransformDelta other)
        {
            Translate += other.Translate;
            Rotate = other.Rotate * Rotate;
            Scale *= other.Scale;
        }
    }
    
    public partial class LibiglBehaviour
    {
        private bool _primaryTransformHand; // True=R
        private bool _isTwoHanded;
        private bool _isTwoHandedPrev;
        
        private bool _doTransformL;
        private bool _doTransformPrevL;
        private bool _doTransformR;
        private bool _doTransformPrevR;
        
        private Vector3 _transformStartHandPosL;
        private Vector3 _transformStartHandPosR;
        private Quaternion _transformStartHandRotL;
        private Quaternion _transformStartHandRotR;

        private const float PressThres = 0.1f;

        // Call this in Update() every frame
        private void UpdateTransform()
        {
            
            // Map inputs to actions
            _doTransformPrevL = _doTransformL;
            _doTransformL = InputManager.Input.GripL > PressThres;
            _doTransformPrevR = _doTransformR;
            _doTransformR = InputManager.Input.GripR > PressThres;
            _isTwoHanded = _doTransformL && _doTransformR;
            _isTwoHandedPrev = _doTransformPrevL && _doTransformPrevR;

            // on pressed
            if (_doTransformL && !_doTransformPrevL)
            {
                // Set start pos/rot for appropriate hand
                _transformStartHandPosL = InputManager.Input.HandPosL;
                _transformStartHandRotL = InputManager.Input.HandRotL;

                if (!_doTransformR)
                    _primaryTransformHand = false;
            }
            
            // on depressed
            if (!_doTransformL && _doTransformPrevL)
            {
                // Add transformation to selection
                if (InputManager.Input.ActiveTool == ToolType.Select)
                    ApplyTransformToSelection();
                
                _primaryTransformHand = true;
            }
            
            // - Right
            // on pressed
            if (_doTransformR && !_doTransformPrevR)
            {
                // Set start pos/rot for appropriate hand
                _transformStartHandPosR = InputManager.Input.HandPosR;
                _transformStartHandRotR = InputManager.Input.HandRotR;

                if (!_doTransformL)
                    _primaryTransformHand = true;
            }
            
            // on depressed
            if (!_doTransformR && _doTransformPrevR)
            {
                // Add transformation to selection
                if (InputManager.Input.ActiveTool == ToolType.Select)
                    ApplyTransformToSelection();
                
                _primaryTransformHand = false;
            }
            
            if(InputManager.Input.ActiveTool == ToolType.Default)
                ApplyTransformToMesh();
        }
        
        
        
        
        

        #region --- Applying TransformDelta

        private void ApplyTransformToMesh()
        {
            if (!_doTransformL && !_doTransformR) return;
            
            // Get & Consume the transformation
            var transformDelta = TransformDelta.Identity();
            GetTransformDelta(ref transformDelta);

            // Apply it to the mesh
            var uTransform = LibiglMesh.transform;
            uTransform.Translate(transformDelta.Translate, Space.World);
            uTransform.rotation = transformDelta.Rotate * uTransform.rotation;
            uTransform.localScale *= transformDelta.Scale;
        }

        private void ApplyTransformToSelection()
        {
            if (!_doTransformL && !_doTransformR) return;

            Input.DoTransform = true;
            GetTransformDelta(ref Input.TransformDelta);
        }

        private void PreExecuteTransform()
        {
            if (InputManager.Input.ActiveTool == ToolType.Select && (_doTransformL || _doTransformR))
            {
                ApplyTransformToSelection();
                
                // Convert to local space
                var uTransform = LibiglMesh.transform;
                Input.TransformDelta.Translate = uTransform.InverseTransformVector(Input.TransformDelta.Translate);
                // Input.TransformDelta.Rotate *= Quaternion.Inverse(uTransform.rotation);
            }
        }
        
        private void ConsumeTransform()
        {
            if(Input.DoTransform)
                Input.TransformDelta = TransformDelta.Identity();

            Input.DoTransform = false;
        }

        private void ResetTransformStartPositions()
        {
            _transformStartHandPosL = InputManager.Input.HandPosL;
            _transformStartHandRotL = InputManager.Input.HandRotL;
            _transformStartHandPosR = InputManager.Input.HandPosR;
            _transformStartHandRotR = InputManager.Input.HandRotR;
        }
        
        #endregion

        
        
        
        
        #region --- Calculating TransformDelta

        // Only for selections, transforming the mesh is done directly
        private void GetTransformDelta(ref TransformDelta transformDelta)
        {
            if(_isTwoHanded)
                GetRotateScaleJoint(ref transformDelta);
            else if(_primaryTransformHand ? _doTransformR : _doTransformL)
                GetTranslate(_primaryTransformHand, ref transformDelta);

            ResetTransformStartPositions();
        }


        private void GetTranslate(bool isRight, ref TransformDelta transformDelta, bool withRotate = true)
        {
            Vector3 translate;
            if (isRight)
                translate = InputManager.Input.HandPosR - _transformStartHandPosR;
            else
                translate = InputManager.Input.HandPosL - _transformStartHandPosL;
            
            var softFactor = isRight ? InputManager.Input.GripR : InputManager.Input.GripL;
            transformDelta.Translate += translate * softFactor;
            
            // Rotate - Optional
            if (!withRotate) return;

            Quaternion rotate;
            if (isRight)
                rotate = InputManager.Input.HandRotR * Quaternion.Inverse(_transformStartHandRotR);
            else
                rotate = InputManager.Input.HandRotL * Quaternion.Inverse(_transformStartHandRotL);

            transformDelta.Rotate = Quaternion.Lerp(Quaternion.identity, rotate, softFactor) * transformDelta.Rotate;
        }

        private void GetRotateScaleJoint(ref TransformDelta transformDelta, bool withRotate = true)
        {
            // Soft editing
            var softFactor = _primaryTransformHand ? InputManager.Input.GripR : InputManager.Input.GripL;
            var softFactorSecondary = !_primaryTransformHand ? InputManager.Input.GripR : InputManager.Input.GripL;

            // Scale
            var scale = (InputManager.Input.HandPosL - InputManager.Input.HandPosR).magnitude / 
                        (_transformStartHandPosL - _transformStartHandPosR).magnitude;
            
            if (float.IsNaN(scale)) scale = 1f;
            scale = (scale -1) * softFactorSecondary + 1;
            
            transformDelta.Scale *= scale;
            
            
            // Rotate - Optional
            if(!withRotate) return;
            
            Vector3 v0, v1;
            if(_primaryTransformHand)
            {
                v0 = _transformStartHandPosR - _transformStartHandPosL;
                v1 = InputManager.Input.HandPosR - InputManager.Input.HandPosL;
            }
            else
            {
                v0 = _transformStartHandPosL - _transformStartHandPosR;
                v1 = InputManager.Input.HandPosL - InputManager.Input.HandPosR;
            }
            
            // Implementation Note: could also use combination of Quaternion.FromToRotation() and Quaternion.Lerp()
            var axis = Vector3.Cross(v0, v1);
            var angle = Vector3.Angle(v0, v1);
            
            angle *= softFactor;
            transformDelta.Rotate = Quaternion.AngleAxis(angle, axis) * transformDelta.Rotate;
        }

        #endregion
    }
}