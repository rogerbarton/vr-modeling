using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl.Behaviour
{
    public partial class LibiglBehaviour
    {
        // Translate
        private bool _actionTranslate;
        
        // Select
        private bool _actionSelect;
        private Vector3 _actionSelectPos;
        private float _actionSelectRadius;
        
        // Harmonic
        private bool _actionHarmonic;
        
        private unsafe void ActionTranslate()
        {
            if (!_actionTranslate) return;
            _actionTranslate = false; // consume

            Native.TranslateMesh((float*) _data.V.GetUnsafePtr(), _data.VSize, new Vector3(0.1f, 0.2f, 0.3f));

            _data.DirtyState |= MeshData.DirtyFlag.VDirty;
        }

        private unsafe void ActionSelect()
        {
            if (!_actionSelect) return;
            _actionSelect = false;
            
            Native.SphereSelect(_state, _data.GetNative(), _actionSelectPos, _actionSelectRadius);
        }

        private unsafe void ActionHarmonic()
        {
            if (!_actionHarmonic) return;
            _actionHarmonic = false;
            
            Native.Harmonic(_state, _data.GetNative());
            _data.DirtyState |= MeshData.DirtyFlag.VDirty;
        }

        private void GenerateActionUI()
        {
            UIActions.get.CreateActionUi("Test", () => Debug.Log("Test"), new []{"test"}, 0);
            UIActions.get.CreateActionUi("Translate", () => { _actionTranslate = true; }, new []{"translate", "move"}, 1);
            UIActions.get.CreateActionUi("Select", () => { _actionSelect = true; }, new [] {"select"});
            UIActions.get.CreateActionUi("Harmonic", () => { _actionHarmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
            
        }
    }
}