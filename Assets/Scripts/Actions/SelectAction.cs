using libigl;
using UnityEngine;

public class SelectAction : IMeshAction
{
    private Vector3 _mousePos;

    public bool ExecuteCondition()
    {
        return Input.GetKey(KeyCode.S);
    }

    public void PreExecute(LibiglMesh libiglMesh)
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

    public void PostExecute(LibiglMesh libiglMesh)
    {
        libiglMesh.Mesh.SetColors(libiglMesh.DataRowMajor.C);
    }
}
