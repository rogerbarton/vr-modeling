using UnityEngine;

namespace libigl
{
    public class SelectAction : IMeshAction
    {
        private Vector3 _mousePos;
        
        public bool ExecuteCondition()
        {
            return Input.GetKey(KeyCode.S);
        }

        public void PreExecute(MeshData libiglMesh)
        {
            _mousePos = Input.mousePosition;
        }

        public void Execute(MeshData data)
        {
            // Get input position
            // Call native function to get selection mask
            // Pass existing mask to add to existing selection
            // Set vertex colors based on mask
            Debug.Log("Selecting");
        }

        public void PostExecute(Mesh mesh, MeshData data)
        {
            mesh.SetColors(data.C);
        }
    }
}