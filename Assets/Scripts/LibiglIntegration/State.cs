using System.Runtime.InteropServices;

namespace libigl.Behaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct State
    {
        public uint DirtyState;

        // Shared with UnityMeshData
        public readonly void* VPtr;
        public readonly void* NPtr;
        public readonly void* CPtr;
        public readonly void* UVPtr;
        public readonly void* FPtr;
        
        public readonly int VSize;
        public readonly int FSize;
        
        // Private C++ state
        public readonly void* S;
    }
}