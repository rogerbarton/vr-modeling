﻿using System;
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
                ActivePivotModeTransform = PivotMode.Hand,
                ActivePivotModeSelect = PivotMode.Hand,
                ActiveSelectionMode = SelectionMode.Add,
                TransformWithRotate = true
            };
        }

        public void TogglePivotMode()
        {
            ActivePivotMode = (PivotMode) ((ActivePivotMode.GetHashCode() + 1) % 
                                           (ActiveTool == ToolType.Transform ? 2 : 3));
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
        Invert
    }

    public enum PivotMode
    {
        Mesh,
        Hand,
        Selection
    }
}