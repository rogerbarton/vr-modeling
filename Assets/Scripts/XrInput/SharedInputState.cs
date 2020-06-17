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
        // World Space Hand Position
        public Vector3 HandPosL;
        public Vector3 HandPosR;
        // World Space Hand Rotation
        public Quaternion HandRotL;
        public Quaternion HandRotR;
        
        public bool primaryBtnL;
        public bool secondaryBtnL;
        public Vector2 primaryAxisL;
        public bool primaryBtnR;
        public bool secondaryBtnR;
        public Vector2 primaryAxisR;
        
        public float BrushRadius;
        
        // Transform
        public TransformMode ActiveTransformMode; 
        public PivotMode ActivePivotMode; 
        
        // Selection
        public SelectionMode ActiveSelectionMode;
        public bool NewSelectionOnDraw; // Draw into a new selection with each stroke

        public static SharedInputState GetInstance()
        {
            return new SharedInputState
            {
                ActiveTool = ToolType.Select, 
                BrushRadius = 0.1f,
                ActiveTransformMode = TransformMode.IndividualTranslateRotate,
                ActivePivotMode = PivotMode.Hand,
                ActiveSelectionMode = SelectionMode.Add
            };
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

    public enum PivotMode
    {
        Mesh,
        Hand,
        Selection
    }

    public enum TransformMode
    {
        /// <summary>
        /// Individual Translate 
        /// </summary>
        IndividualTranslate,

        /// <summary>
        /// Individual Translate+Rotate 
        /// </summary>
        IndividualTranslateRotate,

        /// <summary>
        /// Joint Translate+Rotate 
        /// </summary>
        Joint,

        /// <summary>
        /// Joint Translate+Rotate+Scale 
        /// </summary>
        JointScale,
    }

}