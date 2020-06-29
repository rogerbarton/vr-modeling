using System.Runtime.InteropServices;

namespace Libigl
{
    /// <summary>
    /// Stores pointers to the native arrays in <see cref="UMeshData"/> so we can pass this to C++.
    /// Pointers are to the first element in the respective NativeArray.<br/>
    /// Important: As Native arrays are not managed memory, the underlying array is fixed and will not move due to
    /// Garbage Collection. So an instance's pointers will remain valid.
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