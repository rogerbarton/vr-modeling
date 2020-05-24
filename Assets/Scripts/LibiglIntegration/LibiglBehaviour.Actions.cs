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
            
            scale = (i.HandPosL - i.HandPosR).magnitude - (i.PrevTrafoHandPosL - i.PrevTrafoHandPosR).magnitude;
            
            // Apply soft editing
            var softFactor = i.PrimaryTransformHand ? i.GripL : i.GripR;
            var softFactorSecondary = softFactor * (!i.PrimaryTransformHand ? i.GripL : i.GripR);

            translate *= softFactor;
            scale *= softFactorSecondary;
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
    }
}