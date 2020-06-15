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
        public Quaternion HandRotL;
        public Quaternion HandRotR;
        
        public float BrushRadius;
        
        // Transform
        public TransformMode ActiveTransformMode; 
        public TransformCenter ActiveTransformCenter; 
        
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
                ActiveTransformCenter = TransformCenter.Hand,
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

    public enum TransformCenter
    {
        Mesh,
        Selection,
        Hand
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