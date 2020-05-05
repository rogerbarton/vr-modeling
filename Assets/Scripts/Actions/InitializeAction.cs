using libigl;

namespace Actions
{
    /// <summary>
    /// Action to initialize a mesh when it is loaded
    /// </summary>
    public class InitializeAction : IMeshAction
    {
        private string _meshName = "";

        public bool ExecuteCondition()
        {
            return true;
        }

        public void PreExecute(LibiglMesh libiglMesh)
        {
            _meshName = libiglMesh.name;
        }

        public void Execute(MeshData data)
        {
            var tmp = (uint) data.DirtyState;
            Native.InitializeMesh(_meshName, data.GetNative(), ref tmp);
            data.DirtyState = (MeshData.DirtyFlag) tmp;
        }

        public void PostExecute(LibiglMesh libiglMesh)
        {
        }
    }
}