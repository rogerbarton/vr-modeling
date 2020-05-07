using System.Runtime.InteropServices;

namespace libigl.Behaviour
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct State
    {
        private void* SPtr;
    }
}