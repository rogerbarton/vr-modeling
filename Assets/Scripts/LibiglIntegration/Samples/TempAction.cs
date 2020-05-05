using System;
using UnityEngine;

namespace libigl.Samples
{
    /// <summary>
    /// Example of creating a temporary MeshAction
    /// </summary>
    public class TempAction : MonoBehaviour
    {
        private void Update()
        {
            // return if there is already a job running
            if (MeshManager.ActiveMesh.JobRunning()) return;

            // ExecuteCondition
            if (Input.GetKeyDown(KeyCode.P))
            {
                // PreExecute
                var pos = Input.mousePosition;

                // Execute
                var execute = new Action<MeshData>((MeshData meshData) =>
                {
                    var a = meshData.VSize;
                    // Native.TranslateMesh();
                    Debug.Log($"DynamicAction Execute: a={a}, mousePos={pos}");
                });

                // PostExecute
                var apply = new Action<LibiglMesh>(_ =>
                {
                    Debug.Log("DynamicAction Apply");
                });

                var action = new MeshAction(MeshActionType.OnUpdate, "DynamicSample", execute, default, null, apply).Schedule();
            }
        }
    }
}
