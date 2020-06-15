using UnityEngine;

namespace XrInput
{
    public struct SharedInputState
    {
        // Tools & Input State
        public int ActiveTool;
        
        // Generic Input
        public float GripL;
        public float GripR;
        public Vector3 HandPosL;
        public Vector3 HandPosR;
        
        // Selection
        public SelectionMode ActiveSelectionMode;
        public bool NewSelectionOnDraw; // Draw into a new selection with each stroke

        public static SharedInputState GetInstance()
        {
            return new SharedInputState{ActiveTool = ToolType.Select, ActiveSelectionMode = SelectionMode.Add};
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