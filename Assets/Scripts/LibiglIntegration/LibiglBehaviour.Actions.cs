using UnityEngine;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransformSelection()
        {
            var i = _state->Input;
            if (!i.DoTransform || i.ActiveTool != ToolType.Select) return;

            if (!i.SecondaryTransformHandActive)
            {
                // Only translate selection
                var translate = GetTranslateVector(_state->Input);

                Native.TranslateSelection(_state, translate, i.ActiveSelectionId);
            }
            else
            {
                // Do full transformation
                GetTransformData(_input, out var translate, out var scale, out var angle, out var axis);
                angle *= Mathf.Deg2Rad; // Eigen uses rad, Unity uses deg

                Native.TransformSelection(_state, i.ActiveSelectionId, translate, scale, angle, axis.normalized);
            }
            _state->DirtyState |= DirtyFlag.VDirty;
        }

        /// <summary>
        /// Determines the softenes translation vector
        /// </summary>
        /// <param name="i">The input state to use</param>
        /// <returns></returns>
        private static Vector3 GetTranslateVector(InputState i)
        {
            Vector3 t;
            if(i.PrimaryTransformHand)
                 t = i.HandPosL - i.PrevTrafoHandPosL;
            else
                t = i.HandPosR - i.PrevTrafoHandPosR;
            
            var softFactor = i.PrimaryTransformHand ? i.GripL : i.GripR;
            return t * softFactor;
        }

        /// <summary>
        /// Determines the softened transformation from the input state
        /// </summary>
        /// <param name="i">InputState to use</param>
        /// <param name="angle">In degrees</param>
        private static void GetTransformData(InputState i, out Vector3 translate, out float scale, out float angle, out Vector3 axis)
        {
            Vector3 v0, v1;
            if(i.PrimaryTransformHand)
            {
                translate = i.HandPosL - i.PrevTrafoHandPosL;
                v0 = i.PrevTrafoHandPosL - i.PrevTrafoHandPosR;
                v1 = i.HandPosL - i.HandPosR;
            }
            else
            {
                translate = i.HandPosR - i.PrevTrafoHandPosR;
                v0 = i.PrevTrafoHandPosR - i.PrevTrafoHandPosL;
                v1 = i.HandPosR - i.HandPosL;
            }
                
            axis = Vector3.Cross(v0, v1);
            angle = Vector3.Angle(v0, v1);
            
            // TODO: scale should be done from positions at start of both grips pressed 
            scale = (i.HandPosL - i.HandPosR).magnitude - (i.PrevTrafoHandPosL - i.PrevTrafoHandPosR).magnitude;
            
            // Apply soft editing
            var softFactor = i.PrimaryTransformHand ? i.GripL : i.GripR;
            var softFactorSecondary = softFactor * (!i.PrimaryTransformHand ? i.GripL : i.GripR);

            translate *= softFactor;
            scale = (scale -1) * softFactorSecondary + 1; 
            angle *= softFactorSecondary;
        }

        private void ActionSelect()
        {
            if (!_state->Input.DoSelect) return;
            
            Native.SphereSelect(_state, _state->Input.SelectPos, _state->Input.SelectRadiusSqr, 
                _state->Input.ActiveSelectionId, _state->Input.ActiveSelectionMode);
        }

        private void ActionHarmonic()
        {
            if (!_state->Input.DoHarmonic) return;
            
            Native.Harmonic(_state, -1);
            _state->DirtyState |= DirtyFlag.VDirty;
        }

        private void ActionUI()
        {
            if (_state->Input.DoClearSelection > 0)
            {
                Native.ClearSelection(_state, _state->Input.DoClearSelection);
            }
        }

        private void UpdateMeshTransform()
        {
            var i = _state->Input;
            if (i.ActiveTool == ToolType.Select || !i.DoTransform) return;
            
            // Transform the whole mesh
            if (i.SecondaryTransformHandActive)
            {
                GetTransformData(_input, out var translate, out var scale, out var angle, out var axis);
                var uTransform = _libiglMesh.transform;
                uTransform.Translate(translate);
                uTransform.Rotate(axis, angle);
                uTransform.localScale *= scale;
            }
            else
                _libiglMesh.transform.Translate(GetTranslateVector(_input));
                
            // Consume the input and update the previous position directly
            _input.PrevTrafoHandPosL = _input.HandPosL;
            _input.PrevTrafoHandPosR = _input.HandPosR;
        }
    }
}