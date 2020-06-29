using System;
using UI;
using UnityEngine;

namespace XrInput
{
    /// <summary>
    /// Stores input that is shared between meshes as well as the raw input that has not been mapped to actions.
    /// Raw input has been filtered to prevent conflicts with UI and grabbables.
    /// </summary>
    public struct InputState
    {
        /// <summary>
        /// Which tool are we currently using.
        /// </summary>
        public ToolType ActiveTool;

        // -- Generic Input
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
        
        // -- Transform
        /// <summary>
        /// The pivot mode for the <see cref="ActiveTool"/>
        /// </summary>
        public PivotMode ActivePivotMode
        {
            get => ActiveTool == ToolType.Transform ? ActivePivotModeTransform : ActivePivotModeSelect;
            set
            {
                if(ActivePivotMode == value) return;
                
                if (InputManager.State.ActiveTool == ToolType.Transform)
                    InputManager.State.ActivePivotModeTransform = value;
                else
                    InputManager.State.ActivePivotModeSelect = value;
                
                UiManager.get.PivotMode.Repaint();
            }
        }

        public PivotMode ActivePivotModeTransform;
        public PivotMode ActivePivotModeSelect;
        /// <summary>
        /// Is rotation enabled for transformations.
        /// </summary>
        public bool TransformWithRotate;
        
        // -- Selection
        public SelectionMode ActiveSelectionMode;
        /// <summary>
        /// Draw into a new selection with each stroke
        /// </summary>
        public bool NewSelectionOnDraw;
        /// <summary>
        /// Clear the selection when starting a stroke
        /// </summary>
        public bool DiscardSelectionOnDraw;

        // -- Visual
        /// <summary>
        /// Should we show the bounding boxes of the editable meshes
        /// </summary>
        public bool BoundsVisible;
        
        // -- Tools
        
        /// <summary>
        /// The sub-state of the Transform tool
        /// </summary>
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

        /// <summary>
        /// The sub-state of the Select tool
        /// </summary>
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

        /// <returns>An instance with the defaults set</returns>
        public static InputState GetInstance()
        {
            return new InputState
            {
                ActiveTool = ToolType.Select, 
                BrushRadius = 0.1f,
                ActivePivotModeTransform = PivotMode.Hand,
                ActivePivotModeSelect = PivotMode.Hand,
                ActiveSelectionMode = SelectionMode.Add,
                NewSelectionOnDraw = true,
                TransformWithRotate = true
            };
        }

        /// <summary>
        /// Changes the pivot mode depending on the <see cref="ActiveTool"/>
        /// </summary>
        public void TogglePivotMode()
        {
            ActivePivotMode = (PivotMode) ((ActivePivotMode.GetHashCode() + 1) % 
                                           (ActiveTool == ToolType.Transform ? 2 : 3));
        }
    }

    /// <summary>
    /// Which tool is being used, <see cref="InputState.ActiveTool"/>.
    /// </summary>
    public enum ToolType
    {
        Transform,
        Select
    }

    /// <summary>
    /// The sub-state of the Select tool.
    /// </summary>
    public enum ToolSelectMode
    {
        Idle,
        Selecting,
        TransformingL,
        TransformingR,
        TransformingLr
    }

    /// <summary>
    /// The sub-state of the Transform tool.
    /// </summary>
    public enum ToolTransformMode
    {
        Idle,
        TransformingL,
        TransformingR,
        TransformingLr,
    }

    /// <summary>
    /// How to modify the selection.
    /// </summary>
    public enum SelectionMode
    {
        Add,
        Subtract,
        Invert
    }

    /// <summary>
    /// How to rotate the mesh/selection.
    /// </summary>
    public enum PivotMode
    {
        Mesh,
        Hand,
        Selection
    }
}