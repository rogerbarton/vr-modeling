using System.Runtime.InteropServices;
using UnityEngine;

namespace libigl.Behaviour
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

        public readonly void* VPtr;
        public readonly void* NPtr;
        public readonly void* CPtr;
        public readonly void* UVPtr;
        public readonly void* FPtr;
        
        public readonly int VSize;
        public readonly int FSize;

        // Latest InputState from PreExecute 
        public InputState Input;
        
        // Private C++ state
        public readonly void* S;
        public readonly uint* SSize; // uint[32], vertices selected per selection
        public readonly uint SSizeAll; // Total vertices selected
    }

    /// <summary>
    /// Struct for storing the current input. (This is a value type so assigning will copy).
    /// Anything that may change as we are executing should be in the InputState as it is copied in PreExecute.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct InputState
    {
        public uint ActiveTool;
        
        // Generic Input
        public float GripL;
        public float GripR;
        public Vector3 HandPosL;
        public Vector3 HandPosR;
        // The previous position of the hand when the last transformation was made
        public Vector3 PrevTrafoHandPosL;
        public Vector3 PrevTrafoHandPosR;

        // Transform
        public bool DoTransform;
        public bool PrimaryTransformHand; // True=R
        public bool SecondaryTransformHandActive;

        // Select
        public int ActiveSelectionId;
        public uint ActiveSelectionMode;
        public uint SCount;
        public uint VisibleSelectionMask;

        public bool DoSelect;
        public Vector3 SelectPos;
        public float SelectRadiusSqr;
        // A Mask of the selections that should be cleared
        public uint DoClearSelection; 
        
        // Harmonic
        public bool DoHarmonic;

        public void InitializeDefaults()
        {
            ActiveTool = ToolType.Default;
            SelectRadiusSqr = 0.1f;
        }
        
        public void ChangeActiveSelection(int increment)
        {
            ActiveSelectionId = (int) ((ActiveSelectionId + increment) % SCount);
        }
    }

    public static class ToolType
    {
        public const uint Default = 0;
        public const uint Select = 1;
        public const uint Laser = 2;
        public const uint ViewOnly = 4;
    }

    public static class SelectionMode
    {
        public const uint Add = 0;
        public const uint Subtract = 1;
        public const uint Toggle = 2;
    }
}