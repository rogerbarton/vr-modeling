using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace libigl.Samples
{
    /// <summary>
    /// Example Mesh Action which shows use of <see cref="MeshAction.ExecuteAdvanced"/> to use both column and row major data.
    /// </summary>
    public class AdvancedAction : IMeshActionAdvanced
    {
        public bool ExecuteCondition()
        {
            return Input.GetKeyDown(KeyCode.Alpha0);
        }

        public void PreExecute(MeshData libiglMesh)
        {
        }

        public void ExecuteAdvanced(MeshData dataColMajor, MeshData dataRowMajor)
        {
            // Perform operations on row major version
            // ...

            // Set dirty state, e.g. if V has changed
            dataRowMajor.DirtyState |= MeshData.DirtyFlag.VDirty;

            // Apply changes to column major version
            // You should call this before modifying dataColMajor
            dataRowMajor.ApplyDirtyToTranspose(dataColMajor);

            // Make further changes to the column major version.
            unsafe
            {
                Native.TranslateMesh((float*) dataColMajor.V.GetUnsafePtr(), dataColMajor.VSize,
                    Vector3.forward * 0.1f);
            }

            dataColMajor.DirtyState |= MeshData.DirtyFlag.VDirty;

            // Changes are always automatically applied at the end.
            // The order of modifying col or row major is irrelevant.
        }

        public void PostExecute(Mesh mesh, MeshData data)
        {
            // Only have row major version here to apply changes to the Unity Mesh
            mesh.SetVertices(data.V);
            mesh.RecalculateNormals();
        }
    }
}
