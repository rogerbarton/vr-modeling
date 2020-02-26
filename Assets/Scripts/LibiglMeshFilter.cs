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

        void Start()
        {
            mesh = meshFilter.mesh;
            
            Vector3[] V;
            int[] F;
            //Libigl load mesh by name
            LoadMesh("bunny.off", out V, out F);
            UpdateMeshFilter(V, F);
            
        }

        void Update()
        {

        }

        void LoadMesh(string filename, out Vector3[] V, out int[] F)
        {
            V = new []
            {
                new Vector3(0,0,0), 
                new Vector3(1, 0, 0), 
                new Vector3(0, 1, 0)
            };
            F = new []{0,1,2};
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