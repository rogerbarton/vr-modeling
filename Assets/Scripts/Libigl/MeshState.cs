using System.Runtime.InteropServices;

namespace Libigl
{
    /// <summary>
    /// The <b>C++ state for a mesh</b> in column major.
    /// This is linked to Unity and the RowMajor version via <see cref="UMeshData"/>.
    /// It stores only pointers to the Eigen data so this can be shared between C++ and C#.
    /// The mesh data is allocated in C++ during the <see cref="Native.InitializeMesh"/> function
    /// using <see cref="UMeshDataNative"/>.
    /// <p/>
    /// The <see cref="DirtyState"/> indicates what has been changed and needs to be applied to the Unity mesh.<p/>
    /// As this struct is shared between a managed (C#) and native (C++) context,
    /// you must consider Marshalling when adding new variables.
    /// <remarks><c>void*</c> usually represents an Eigen matrix, but can be anything.</remarks>
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct MeshState
    {
        /// <summary>
        /// Tells us what has changed with the mesh using the <see cref="DirtyFlag"/> constants
        /// </summary>
        public uint DirtyState;
        /// <summary>
        /// Tells us which selections have been modified, as a bitmask. Each bit represents one selection.
        /// </summary>
        public uint DirtySelections;
        /// <summary>
        /// Less stricter version than <see cref="DirtySelections"/>,
        /// where we only consider a selection dirty if the selected vertices size changes, see <see cref="SSizes"/>.
        /// </summary>
        public uint DirtySelectionsResized;

        // -- ColMajor Mesh data Shared with UnityMeshData
        public readonly void* VPtr;
        public readonly void* NPtr;
        public readonly void* CPtr;
        public readonly void* UVPtr;
        public readonly void* FPtr;

        public readonly int VSize;
        public readonly int FSize;

        public readonly void* SPtr;
        /// <summary>
        /// Amount of selections enabled
        /// </summary>
        public uint SSize;
        /// <summary>
        /// uint[32], vertices selected per selection
        /// </summary>
        public readonly uint* SSizes;
        /// <summary>
        /// Total vertices selected
        /// </summary>
        public readonly uint SSizesAll;

        /// <summary>
        /// Native only state
        /// </summary>
        private readonly void* Native;
    }
}