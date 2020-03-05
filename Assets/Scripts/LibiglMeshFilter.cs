using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace libigl.rendering
{
    public class LibiglMeshFilter : MonoBehaviour
    {
        [Tooltip("Mesh to update with V, F, uv matrices")]
        public MeshFilter meshFilter;
        private Mesh mesh;

        public Vector3 direction;//for testing

        void Awake()
        {
            mesh = meshFilter.mesh;
        }

        private void Start()
        {
            //TODO: Convert Vector3[] to float* which we can pass to cpp
            // NativeArray<NativeArray<float>> V = mesh.vertices;
            
            // unsafe {Native.moveVert(V, V.Length);}
            mesh.MarkDynamic(); //for efficiency, 'makes buffers CPU writable'
            var submesh = mesh.GetSubMesh(0);
            submesh.topology = MeshTopology.Triangles;
        }

        private void Update()
        {
            var V = mesh.GetNativeVertexBufferPtr(0);
            if (Input.GetKeyDown(KeyCode.Space))
                Native.MoveV(V, mesh.vertexCount, new []{direction.x, direction.y, direction.z});
        }

        public void UpdateMeshFilter(Vector3[] V, int[] F)
        {
            if(mesh.vertices.Length != V.Length || mesh.triangles.Length != F.Length / 3)
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