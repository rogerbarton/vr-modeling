using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace libigl.rendering
{
    public class LibiglMeshFilter : MonoBehaviour
    {
        [Tooltip("Mesh to update with V, F, uv matrices")]
        public MeshFilter meshFilter;
        private Mesh mesh;

        void Awake()
        {
            mesh = meshFilter.mesh;
        }

        public void UpdateMeshFilter(Vector3[] V, int[] F)
        {
            mesh.Clear();
            
            mesh.vertices = V;
            mesh.triangles = F;
        }
        
        public void UpdateMeshFilter(Vector3[] V, int[] F, Vector2[] uv)
        {
            mesh.vertices = V;
            mesh.uv = uv;
            mesh.triangles = F;
        }
    }
}