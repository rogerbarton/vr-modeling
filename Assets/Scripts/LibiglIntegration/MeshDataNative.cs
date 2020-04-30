using System.Runtime.InteropServices;

namespace libigl
{
    /// <summary>
    /// Value type that stores all pointers for MeshData
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly unsafe struct MeshDataNative
    {
        public readonly float* V;
        public readonly float* N;
        public readonly float* C;
        public readonly float* UV;
        public readonly int* F;
        public readonly int VSize;
        public readonly int FSize;

        public MeshDataNative(float* v, float* n, float* c, float* uv, int* f, int vSize, int fSize)
        {
            V = v;
            N = n;
            C = c;
            UV = uv;
            F = f;
            VSize = vSize;
            FSize = fSize;
        }
    }
}