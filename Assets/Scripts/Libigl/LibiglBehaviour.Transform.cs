using System;
using UnityEngine;
using UnityEngine.XR;
using Util;
using XrInput;

namespace Libigl
{
    /// <summary>
    /// Stores information about a single transformation.
    /// </summary>
    public struct TransformDelta
    {
        public Vector3 Translate;
        public Quaternion Rotate;
        public float Scale;

        public PivotMode Mode;
        public Vector3 Pivot;
        
        public static TransformDelta Identity()
        {
            return new TransformDelta
            {
                Rotate = Quaternion.identity,
                Scale = 1f,
                Mode = InputManager.State.ActivePivotMode
            };
        }

        /// <summary>
        /// (experimental) Combines two transformations, does not consider the pivot. 
        /// </summary>
        public void Add(TransformDelta other)
        {
            Translate += other.Translate;
            Rotate = other.Rotate * Rotate;
            Scale *= other.Scale;
        }
    }

    public partial class LibiglBehaviour
    {
        /// <summary>
        /// The first hand to start a transformation.
        /// True = Right
        /// </summary>
        private bool _firstTransformHand;
        /// <summary>
        /// Are we using both hands in the transformation
        /// </summary>
        private bool _isTwoHandedTransformation;
        private bool _isTwoHandedTransformationPrev;

        private bool _doTransformL;
        private bool _doTransformPrevL;
        private bool _doTransformR;
        private bool _doTransformPrevR;

        /// <summary>
        /// Where the hand was when the <see cref="TransformDelta"/> was started.
        /// Or the hand position at the last time the transformation was consumed.
        /// </summary>
        private Vector3 _transformStartHandPosL;
        private Vector3 _transformStartHandPosR;
        private Quaternion _transformStartHandRotL;
        private Quaternion _transformStartHandRotR;

        /// <summary>
        /// At which point do we consider the button as pressed
        /// </summary>
        private const float GrabPressThreshold = 0.1f;

        /// <summary>
        /// Updates the current transformation state from the input, regardless of what the worker thread is doing.
        /// Decides when a transformation is started/stopped (changes in the finite state machine FSM).
        /// Applies the transformation immediately if we are transforming the mesh (as this is done by Unity).
        /// Call this in Update() every frame.
        /// </summary>
        private void UpdateTransform()
        {
            // Map inputs to actions
            _doTransformPrevL = _doTransformL;
            _doTransformL = InputManager.State.GripL > GrabPressThreshold;
            _doTransformPrevR = _doTransformR;
            _doTransformR = InputManager.State.GripR > GrabPressThreshold;
            _isTwoHandedTransformation = _doTransformL && _doTransformR;
            _isTwoHandedTransformationPrev = _doTransformPrevL && _doTransformPrevR;

            // - Left
            // on pressed
            if (_doTransformL && !_doTransformPrevL)
            {
                // Set start pos/rot for appropriate hand
                _transformStartHandPosL = InputManager.State.HandPosL;
                _transformStartHandRotL = InputManager.State.HandRotL;

                if (!_doTransformR)
                    _firstTransformHand = false;
            }

            // on depressed
            if (!_doTransformL && _doTransformPrevL)
            {
                // Add transformation to selection
                if (InputManager.State.ActiveTool == ToolType.Select)
                    ApplyTransformToSelection();

                _firstTransformHand = true;
            }

            // - Right
            // on pressed
            if (_doTransformR && !_doTransformPrevR)
            {
                // Set start pos/rot for appropriate hand
                _transformStartHandPosR = InputManager.State.HandPosR;
                _transformStartHandRotR = InputManager.State.HandRotR;

                if (!_doTransformL)
                    _firstTransformHand = true;
            }

            // on depressed
            if (!_doTransformR && _doTransformPrevR)
            {
                // Add transformation to selection
                if (InputManager.State.ActiveTool == ToolType.Select)
                    ApplyTransformToSelection();

                _firstTransformHand = false;
            }

            // Immediately apply if we are moving the mesh
            if (InputManager.State.ActiveTool == ToolType.Transform)
                ApplyTransformToMesh();

            // Additional functionality of the FSM
            if (_doTransformL || _doTransformR)
            {
                if (InputManager.State.PrimaryBtnR && !InputManager.StatePrev.PrimaryBtnR)
                    InputManager.State.TransformWithRotate = !InputManager.State.TransformWithRotate;

                if (InputManager.State.SecondaryBtnR && !InputManager.StatePrev.SecondaryBtnR)
                    InputManager.State.TogglePivotMode();
            }
        }

        
        #region --- Applying TransformDelta

        /// <summary>
        /// Gets and consumes the transformation, applying it to the LibiglMesh <see cref="Transform"/>.
        /// </summary>
        private void ApplyTransformToMesh()
        {
            if (!_doTransformL && !_doTransformR) return;

            var transformDelta = TransformDelta.Identity();
            if (transformDelta.Mode == PivotMode.Selection)
            {
                Debug.LogWarning(
                    "Invalid pivot mode PivotMode.Selection for transforming the mesh, using PivotMode.Hand.");
                transformDelta.Mode = PivotMode.Hand;
            }

            // Get & Consume the transformation
            GetTransformDelta(true, ref transformDelta, Space.World, InputManager.State.TransformWithRotate,
                _isTwoHandedTransformation, _firstTransformHand);

            // Apply it to the mesh
            var uTransform = Mesh.transform;
            uTransform.Translate(transformDelta.Translate, Space.World);
            uTransform.localScale *= transformDelta.Scale;

            var pivotLocal = uTransform.localScale.CwiseMul(uTransform.InverseTransformPoint(transformDelta.Pivot));
            if (transformDelta.Mode != PivotMode.Mesh &&
                InputManager.State.ToolTransformMode != ToolTransformMode.TransformingLr)
                uTransform.position += uTransform.rotation * pivotLocal;

            uTransform.rotation = transformDelta.Rotate * uTransform.rotation;

            if (transformDelta.Mode != PivotMode.Mesh &&
                InputManager.State.ToolTransformMode != ToolTransformMode.TransformingLr)
                uTransform.position -= uTransform.rotation * pivotLocal;

            // This should draw a line from the mesh center to the pivot
            // Debug.DrawRay(transformDelta.Pivot, uTransform.rotation * -pivotLocal, Color.magenta);
        }

        /// <summary>
        /// Save and consume the transformation, which will later be applied to the selection on the worker thread.
        /// </summary>
        private void ApplyTransformToSelection()
        {
            if (!_doTransformL && !_doTransformR) return;

            Input.DoTransformL |= _doTransformL;
            Input.DoTransformR |= _doTransformR;
            Input.PrimaryTransformHand = _firstTransformHand;

            // Calculate the individual transforms as well as the two handed transformation,
            // as we will decide later which one is used
            if (Input.DoTransformL && Input.DoTransformR)
                // Implementation note: were accessing _currentTranslateMaskL from both threads (!)
                // if we newly started two handed mode or the masks are different. Disable this efficiency gain for now for reliability
                // &&(!Input.DoTransformLPrev || !Input.DoTransformLPrev ||
                // _currentTranslateMaskL != _currentTranslateMaskR))
            {
                GetTransformDelta(false, ref Input.TransformDeltaL, Space.Self, InputManager.State.TransformWithRotate,
                    false,
                    false);
                GetTransformDelta(false, ref Input.TransformDeltaR, Space.Self, InputManager.State.TransformWithRotate,
                    false,
                    true);
            }

            // Consume on last
            GetTransformDelta(true, ref Input.TransformDeltaJoint, Space.Self, InputManager.State.TransformWithRotate,
                _isTwoHandedTransformation,
                _firstTransformHand);
        }

        private void PreExecuteTransform()
        {
            // Consume the transformation
            // If we have already done this this should just add the identity
            if (InputManager.State.ActiveTool == ToolType.Select)
            {
                ApplyTransformToSelection();
            }
        }

        private void ResetTransformStartPositions()
        {
            _transformStartHandPosL = InputManager.State.HandPosL;
            _transformStartHandRotL = InputManager.State.HandRotL;
            _transformStartHandPosR = InputManager.State.HandPosR;
            _transformStartHandRotR = InputManager.State.HandRotR;
        }

        #endregion
        

        #region --- Calculating TransformDelta

        /// <summary>
        /// Find out the <see cref="TransformDelta"/> that should be done.
        /// Independent of what we are transforming, mesh or selection.
        /// </summary>
        /// <param name="consumeInput">Should we reset the starting positions/rotations of the hands.
        /// This is unrelated to the <see cref="MeshInputState.Consume"/> function which is for the worker thread</param>
        /// <param name="transformDelta">Where we should add our transformation to.</param>
        private void GetTransformDelta(bool consumeInput, ref TransformDelta transformDelta, Space space,
            bool withRotate, bool isTwoHanded, bool primaryHand)
        {
            if (isTwoHanded)
                GetTransformTwoHanded(ref transformDelta, withRotate);
            else if (primaryHand ? _doTransformR : _doTransformL)
                GetTransformOneHanded(primaryHand, ref transformDelta, withRotate);

            // Update pivot to latest (overwrites for now)
            if (InputManager.State.ActivePivotMode == PivotMode.Hand)
                transformDelta.Pivot = primaryHand ? InputManager.State.HandPosR : InputManager.State.HandPosL;
            else
                transformDelta.Pivot = Mesh.transform.position;

            if (space == Space.Self)
            {
                // Conversions to local space
                var uTransform = Mesh.transform;
                transformDelta.Translate = uTransform.InverseTransformVector(transformDelta.Translate);
                transformDelta.Pivot = uTransform.InverseTransformPoint(transformDelta.Pivot);
                transformDelta.Rotate =
                    Quaternion.Inverse(uTransform.rotation) * transformDelta.Rotate * uTransform.rotation;
            }

            if (consumeInput)
                ResetTransformStartPositions();
        }

        /// <summary>
        /// Finds out the transformation when using one hand
        /// </summary>
        private void GetTransformOneHanded(bool isRight, ref TransformDelta transformDelta, bool withRotate = true)
        {
            Vector3 translate;
            if (isRight)
                translate = InputManager.State.HandPosR - _transformStartHandPosR;
            else
                translate = InputManager.State.HandPosL - _transformStartHandPosL;

            var softFactor = isRight ? InputManager.State.GripR : InputManager.State.GripL;
            transformDelta.Translate += translate * softFactor;

            // Rotate - Optional
            if (!withRotate) return;

            Quaternion rotate;
            if (isRight)
                rotate = InputManager.State.HandRotR * Quaternion.Inverse(_transformStartHandRotR);
            else
                rotate = InputManager.State.HandRotL * Quaternion.Inverse(_transformStartHandRotL);

            transformDelta.Rotate = Quaternion.Lerp(Quaternion.identity, rotate, softFactor) * transformDelta.Rotate;
        }

        /// <summary>
        /// Finds out the transformation when using both hands
        /// </summary>
        private void GetTransformTwoHanded(ref TransformDelta transformDelta, bool withRotate = true)
        {
            // Soft editing
            var softFactor = _firstTransformHand ? InputManager.State.GripR : InputManager.State.GripL;
            var softFactorSecondary = !_firstTransformHand ? InputManager.State.GripR : InputManager.State.GripL;

            // Scale
            var scale = (InputManager.State.HandPosL - InputManager.State.HandPosR).magnitude /
                        (_transformStartHandPosL - _transformStartHandPosR).magnitude;

            if (float.IsNaN(scale)) scale = 1f;
            scale = (scale - 1) * softFactorSecondary + 1;

            transformDelta.Scale *= scale;


            // Rotate - Optional
            if (!withRotate) return;

            Vector3 v0, v1;
            if (_firstTransformHand)
            {
                v0 = _transformStartHandPosR - _transformStartHandPosL;
                v1 = InputManager.State.HandPosR - InputManager.State.HandPosL;
            }
            else
            {
                v0 = _transformStartHandPosL - _transformStartHandPosR;
                v1 = InputManager.State.HandPosL - InputManager.State.HandPosR;
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