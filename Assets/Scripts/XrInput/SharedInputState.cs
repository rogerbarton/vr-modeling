using System;
using UI;
using UnityEngine;

namespace XrInput
{
    public struct SharedInputState
    {
        // Tools & Input State
        public ToolType ActiveTool;

        public ToolTransformMode ToolTransformMode
        {
            get => _toolTransformMode;
            set
            {
                if (_toolTransformMode == value) return;
                _toolTransformMode = value;
                
                InputManager.get.RepaintInputHints();
            }
        }

        private ToolTransformMode _toolTransformMode;

        public ToolSelectMode ToolSelectMode
        {
            get => _toolSelectMode;
            set
            {
                if (_toolSelectMode == value) return;
                _toolSelectMode = value;
                
                InputManager.get.RepaintInputHints();        
            }
        }
        private ToolSelectMode _toolSelectMode;
        
        // Generic Input
        public float GripL;
        public float TriggerL;
        public float GripR;
        public float TriggerR;
        // World Space Hand Position
        public Vector3 HandPosL;
        public Vector3 HandPosR;
        // World Space Hand Rotation
        public Quaternion HandRotL;
        public Quaternion HandRotR;
        
        public bool PrimaryBtnL;
        public bool SecondaryBtnL;
        public Vector2 PrimaryAxisL;
        public bool PrimaryBtnR;
        public bool SecondaryBtnR;
        public Vector2 PrimaryAxisR;
        
        public float BrushRadius;
        
        // Transform
        public TransformMode ActiveTransformMode; 
        public PivotMode ActivePivotMode;
        public bool TransformWithRotate;
        
        // Selection
        public SelectionMode ActiveSelectionMode;
        public bool NewSelectionOnDraw; // Draw into a new selection with each stroke
        public bool DiscardSelectionOnDraw; // Clear the selection when starting a stroke

        // Visual
        public bool BoundsVisible;

        public static SharedInputState GetInstance()
        {
            return new SharedInputState
            {
                ActiveTool = ToolType.Select, 
                BrushRadius = 0.1f,
                ActiveTransformMode = TransformMode.IndividualTranslateRotate,
                ActivePivotMode = PivotMode.Hand,
                ActiveSelectionMode = SelectionMode.Add,
                TransformWithRotate = true
            };
        }
    }

    public enum ToolSelectMode
    {
        Idle,
        Selecting,
        TransformingL,
        TransformingR,
        TransformingLR
    }

    public enum ToolTransformMode
    {
        Idle,
        TransformingL,
        TransformingR,
        TransformingLR,
    }

    public enum ToolType
    {
        Transform,
        Select
    }

    public enum SelectionMode
    {
        Add,
        Subtract,
        Invert,
        Discard
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