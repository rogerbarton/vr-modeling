using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl
{
    /// <summary>
    /// Example Mesh Action which stores data for the worker thread <see cref="_deltaTime"/>.
    /// </summary>
    public class TranslateAction : IMeshAction
    {
        private float _deltaTime;
        
        public bool ExecuteCondition()
        {
            return Input.GetKeyDown(KeyCode.W);
        }

        public void PreExecute(MeshData libiglMesh)
        {
            _deltaTime = Time.deltaTime;
            Debug.Log($"PreExecute dt: {_deltaTime}");
        }
        
        public void Execute(MeshData data)
        {
            Debug.Log($"Execute dt: {_deltaTime}");
            unsafe
            {
                Native.TranslateMesh((float*) data.V.GetUnsafePtr(), data.VSize, Vector3.forward * 0.1f);
            }
        }

        public void PostExecute(Mesh mesh, MeshData data)
        {
            Debug.Log($"PostExecute dt: {_deltaTime}");

            mesh.SetVertices(data.V);
            mesh.RecalculateNormals();
        }
    }
}