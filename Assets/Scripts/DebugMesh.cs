using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class DebugMesh : MonoBehaviour
{
    private Mesh mesh;
    public VertexAttributeDescriptor[] layout;
    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        layout = mesh.GetVertexAttributes();
        var length = layout.Length;
        
    }
}
