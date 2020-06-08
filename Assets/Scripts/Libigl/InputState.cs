using UnityEngine;

namespace Libigl
{
    
    /// <summary>
    /// Struct for storing the current input. (This is a value type so assigning will copy).
    /// Anything that may change as we are executing should be in the InputState as it is copied in PreExecute.
    /// </summary>
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
        public SelectionMode ActiveSelectionMode;
        public bool NewSelectionOnDraw; // Draw into a new selection with each stroke
        public uint VisibleSelectionMask;
        public bool VisibleSelectionMaskChanged;
        public uint SCountUi; // For UI, will be copied to the state in PreExecute

        public bool DoSelect;
        public bool DoSelectStarted;
        public Vector3 SelectPos;
        public float SelectRadiusSqr;
        // A Mask of the selections that should be cleared
        public uint DoClearSelection; 
        
        // Deformations
        public bool DoHarmonicOnce; // Trigger execution once
        public bool DoHarmonic;     // Trigger execution every frame
        public bool HarmonicShowDisplacement;
        public bool DoArapOnce;
        public bool DoArap;

        /// <returns>An instance with the default values</returns>
        public static InputState GetInstance()
        {
            return new InputState {ActiveTool = ToolType.Select, VisibleSelectionMask = unchecked((uint)-1), ActiveSelectionMode = SelectionMode.Add, SelectRadiusSqr = 0.1f};
        }
        
        public void ChangeActiveSelection(int increment)
        {
            ActiveSelectionId = (int) ((ActiveSelectionId + increment) % SCountUi);
        }
    }

    public static class ToolType
    {
        public const uint Default = 0;
        public const uint Select = 1;
        public const uint Laser = 2;
        public const uint ViewOnly = 4;
    }

    public enum SelectionMode
    {
        Add,
        Subtract,
        Toggle
    }
}