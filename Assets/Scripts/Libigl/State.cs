using System.Runtime.InteropServices;

namespace Libigl
{
    /// <summary>
    /// The C++ state for a mesh, Stores only pointers so this can be shared between C++ and C#.<br/>
    /// A subset of the variables represents the ColMajor version of <see cref="UMeshData"/> and the <see cref="DirtyState"/>
    /// indicates which of these have been changed and need to be applied to the Unity mesh.<br/>
    /// As this struct is shared between a managed (C#) and native (C++) context you must consider Marshalling when adding new variables.
    /// <remarks><c>void*</c> usually represents an Eigen matrix.</remarks>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct State
    {
        // ColMajor Mesh data Shared with UnityMeshData
        public uint DirtyState;
        public uint DirtySelections;
        public uint DirtySelectionsResized;

        public readonly void* VPtr;
        public readonly void* NPtr;
        public readonly void* CPtr;
        public readonly void* UVPtr;
        public readonly void* FPtr;

        public readonly int VSize;
        public readonly int FSize;

        public readonly void* S;
        public readonly uint* SSize; // uint[32], vertices selected per selection
        public readonly uint SSizeAll; // Total vertices selected
        public uint SCount; // Amount of selections

        // Native only state
        private readonly void* Native;
    }
}