using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl.Behaviour
{
    public partial class LibiglBehaviour
    {
        private unsafe void ActionTranslate()
        {
            if (!_actionTranslate) return;

            Native.TranslateMesh((float*) _data.V.GetUnsafePtr(), _data.VSize, new Vector3(0.1f, 0.2f, 0.3f));

            _data.DirtyState |= MeshData.DirtyFlag.VDirty;
        }

        private unsafe void ActionSelect()
        {
            if (!_actionSelect) return;
            
            Native.SphereSelect(_state, _data.GetNative(), _actionSelectPos, _actionSelectRadius);
        }

        private void ActionHarmonic()
        {
            
        }
    }
}