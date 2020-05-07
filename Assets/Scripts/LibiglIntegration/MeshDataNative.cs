using System.Runtime.InteropServices;

namespace libigl
{
    /// <summary>
    /// Value type that stores all pointers for MeshData
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MeshDataNative
    {
        public MeshData.DirtyFlag DirtyState;
        
        public readonly float* VPtr;
        public readonly float* NPtr;
        public readonly float* CPtr;
        public readonly float* UVPtr;
        public readonly int* FPtr;
        
        public readonly int VSize;
        public readonly int FSize;

        public MeshDataNative(MeshData.DirtyFlag dirtyState, float* vPtr, float* nPtr, float* cPtr, float* uvPtr, int* fPtr, int vSize, int fSize)
        {
            DirtyState = dirtyState;
            VPtr = vPtr;
            NPtr = nPtr;
            CPtr = cPtr;
            UVPtr = uvPtr;
            FPtr = fPtr;
            VSize = vSize;
            FSize = fSize;
        }
    }
}