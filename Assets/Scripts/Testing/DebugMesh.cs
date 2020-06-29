using UnityEngine;
using UnityEngine.Rendering;

namespace Testing
{
    public class DebugMesh : MonoBehaviour
    {
        private Mesh _mesh;
        public VertexAttributeDescriptor[] layout;

        private void Start()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            var ptr = _mesh.GetNativeIndexBufferPtr();
            Debug.Log("IsReadable " + _mesh.isReadable);
            _mesh.MarkDynamic();
            layout = _mesh.GetVertexAttributes();
            var length = layout.Length;

        }
    }
}