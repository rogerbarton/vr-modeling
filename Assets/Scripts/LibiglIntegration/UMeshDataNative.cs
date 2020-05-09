using System.Runtime.InteropServices;

namespace libigl
{
    /// <summary>
    /// Value type that stores all pointers for MeshData
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct UMeshDataNative
    {
        public readonly float* VPtr;
        public readonly float* NPtr;
        public readonly float* CPtr;
        public readonly float* UVPtr;
        public readonly int* FPtr;
        
        public readonly int VSize;
        public readonly int FSize;

        public UMeshDataNative(float* vPtr, float* nPtr, float* cPtr, float* uvPtr, int* fPtr, int vSize, int fSize)
        {
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