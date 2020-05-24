using UnityEngine;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTransform()
        {
            if (!_state->Input.DoTransform) return;

            var i = _state->Input;
            Vector3 translate, axis, v0, v1;
            float angle;
            if(i.PrimaryTransformHand)
            {
                translate = i.HandPosL - i.PrevHandPosL;
                v0 = i.PrevHandPosL - i.PrevHandPosR;
                v1 = i.HandPosL - i.HandPosR;
            }
            else
            {
                translate = i.HandPosR - i.PrevHandPosR;
                v0 = i.PrevHandPosR - i.PrevHandPosL;
                v1 = i.HandPosR - i.HandPosL;
            }
                
            axis = Vector3.Cross(v0, v1);
            angle = Vector3.Angle(v0, v1);

            var scale = (i.HandPosL - i.HandPosR).magnitude - (i.PrevHandPosL - i.PrevHandPosR).magnitude;
            
            // Apply either to selection or Unity Transform
            if(_state->Input.ActiveTool == ToolType.Select)
            {
                // Native.TranslateMesh(_state, new Vector3(0.1f, 0.2f, 0.3f));
                Native.TransformSelection(_state, -1, translate, scale, angle, axis.normalized);
                _state->DirtyState |= DirtyFlag.VDirty;
            }
            else
            {
                //TODO: move this to the main thread, in Behaviour.Update()
                var uTransform = _libiglMesh.transform;
                uTransform.Translate(translate);
                uTransform.Rotate(axis, angle);
                uTransform.localScale *= scale;
            }
        }

        private void ActionSelect()
        {
            if (!_state->Input.DoSelect) return;
            
            Native.SphereSelect(_state, _state->Input.SelectPos, _state->Input.SelectRadiusSqr, _state->Input.SelectActiveId);
        }

        private void ActionHarmonic()
        {
            if (!_state->Input.DoHarmonic) return;
            
            Native.Harmonic(_state);
            _state->DirtyState |= DirtyFlag.VDirty;
        }
    }
}