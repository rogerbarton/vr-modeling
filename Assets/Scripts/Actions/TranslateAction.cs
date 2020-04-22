using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl
{
    public class TranslateAction : IExecuteApply
    {
        public void Execute(MeshData data)
        {
            unsafe
            {
                Native.TranslateMesh(data.V.GetUnsafePtr(), data.VSize, Vector3.forward * 0.1f);
            }
        }

        public void Apply(Mesh mesh, MeshData data)
        {
            mesh.SetVertices(data.V);
            mesh.RecalculateNormals();
        }
    }
}