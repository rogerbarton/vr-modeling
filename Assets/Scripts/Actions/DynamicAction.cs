using System;
using System.Threading;
using UnityEngine;

namespace libigl
{
    /// <summary>
    /// Example of creating a MeshAction dynamically with manual entry points (i.e. no UI generation)
    /// </summary>
    public class DynamicAction : MonoBehaviour
    {
        private void Update()
        {
            // return if there is already a job running
            if (MeshManager.activeMesh.JobRunning()) return;
         
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
                var apply = new Action<Mesh, MeshData>((m, d) =>
                {
                    Debug.Log("DynamicAction Apply");
                    m.SetVertices(d.V);
                });
                    
                var action = new MeshAction("Temp", execute, apply).Schedule();
            }
        }
    }
}