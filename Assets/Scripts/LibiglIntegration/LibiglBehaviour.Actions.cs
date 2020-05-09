using UnityEngine;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void ActionTranslate()
        {
            if (!_state->Input.Translate) return;

            Native.TranslateMesh(_state, new Vector3(0.1f, 0.2f, 0.3f));

            _state->DirtyState |= DirtyFlag.VDirty;
        }

        private void ActionSelect()
        {
            if (!_state->Input.Select) return;
            
            Native.SphereSelect(_state, _state->Input.SelectPos, _state->Input.SelectRadiusSqr);
        }

        private void ActionHarmonic()
        {
            if (!_state->Input.Harmonic) return;
            
            Native.Harmonic(_state);
            _state->DirtyState |= DirtyFlag.VDirty;
        }
    }
}