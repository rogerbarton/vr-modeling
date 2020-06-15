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
                ActiveTransformMode = TransformMode.TwoHandedNoScale,
                ActiveTransformCenter = TransformCenter.Selection,
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
        /// 1. One Handed Translate
        /// </summary>
        OneHandedTranslate,

        /// <summary>
        /// 2. One Handed Translate+Rotate
        /// </summary>
        OneHandedTranslateRotate,

        /// <summary>
        /// 3. Two Handed individual Translate 
        /// </summary>
        TwoHandedIndividualTranslate,

        /// <summary>
        /// 4. Two Handed individual Translate+Rotate 
        /// </summary>
        TwoHandedIndividualTranslateRotate,

        /// <summary>
        /// 5. Two Handed joint Translate+Rotate 
        /// </summary>
        TwoHandedJoint,

        /// <summary>
        /// 6. Two Handed joint Translate+Rotate+Scale 
        /// </summary>
        TwoHandedJointScale,
    }

}