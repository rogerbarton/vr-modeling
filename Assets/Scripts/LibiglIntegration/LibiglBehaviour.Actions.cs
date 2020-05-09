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
        private float _actionSelectRadiusSqr;
        
        // Harmonic
        private bool _actionHarmonic;
        
        private unsafe void ActionTranslate()
        {
            if (!_actionTranslate) return;
            _actionTranslate = false; // consume

            Native.TranslateMesh(_state, new Vector3(0.1f, 0.2f, 0.3f));

            _state->DirtyState |= UMeshData.DirtyFlag.VDirty;
        }

        private unsafe void ActionSelect()
        {
            if (!_actionSelect) return;
            _actionSelect = false;
            
            Native.SphereSelect(_state, _actionSelectPos, _actionSelectRadiusSqr);
        }

        private unsafe void ActionHarmonic()
        {
            if (!_actionHarmonic) return;
            _actionHarmonic = false;
            
            Native.Harmonic(_state);
            _state->DirtyState |= UMeshData.DirtyFlag.VDirty;
        }

        
        /// <summary>
        /// Static method that generates the UI to <i>manipulate the active mesh</i>
        /// </summary>
        public static void InitializeActionUi()
        {
            UIActions.get.CreateActionUi("Test", () => Debug.Log("Test"), new []{"test"}, 0);
            UIActions.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour._actionTranslate = true; }, new []{"translate", "move"}, 1);
            UIActions.get.CreateActionUi("Select", () => { MeshManager.ActiveMesh.Behaviour._actionSelect = true; }, new [] {"select"});
            UIActions.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour._actionHarmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
        }
    }
}