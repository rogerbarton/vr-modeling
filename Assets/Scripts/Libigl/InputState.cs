using UnityEngine;
using XrInput;

namespace Libigl
{
    
    /// <summary>
    /// Struct for storing the current input. (This is a value type so assigning will copy).
    /// Anything that may change as we are executing should be in the InputState as it is copied in PreExecute.
    /// </summary>
    public struct InputState
    {
        /// <summary>
        /// Copy for threading of <see cref="InputManager.ActiveTool"/>
        /// </summary>
        public int ActiveTool { get; private set; }

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
        public bool DoTransformPrev;
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
        public bool DoSelectPrev;
        public Vector3 SelectPos;
        public float SelectRadius;
        // A Mask of the selections that should be cleared
        public uint DoClearSelection; 
        
        // Deformations
        public bool DoHarmonic; // Trigger execution once
        public bool DoHarmonicRepeat; // Trigger execution every frame
        public bool HarmonicShowDisplacement;
        public bool DoArap;
        public bool DoArapRepeat;

        public bool ResetV;

        /// <returns>An instance with the default values</returns>
        public static InputState GetInstance()
        {
            return new InputState {VisibleSelectionMask = unchecked((uint)-1), ActiveSelectionMode = SelectionMode.Add, SelectRadius = 0.1f, HarmonicShowDisplacement = true};
        }
        
        public void ChangeActiveSelection(int increment)
        {
            ActiveSelectionId = (int) ((ActiveSelectionId + increment) % SCountUi);
        }

        /// <summary>
        /// Only for private items of InputState
        /// </summary>
        public void PreExecute()
        {
            // Copy state from the input manager
            ActiveTool = InputManager.Input.ActiveTool;
        }
    }

    public static class ToolType
    {
        public const int Invalid = -1;
        public const int Default = 0;
        public const int Select = 1;
        // public const int Laser = 2;
        // public const int ViewOnly = 3;

        public const int Size = 2;
    }

    public enum SelectionMode
    {
        Add,
        Subtract,
        Toggle
    }
}