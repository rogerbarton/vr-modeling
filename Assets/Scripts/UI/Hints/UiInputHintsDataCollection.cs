using UnityEngine;

namespace UI.Hints
{
    /// <summary>
    /// Stores the hints for all possible states of the ActiveTool and sub-states.
    /// These are stored as a hierarchy. <see cref="UiInputHints.Repaint"/> defines how this data is applied.
    /// There will be one instance for the left and one for the right hand.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Input Hint Data Collection")]
    public class UiInputHintsDataCollection : ScriptableObject
    {
        [Header("Default Input Hints")]
        public UiInputHintsData defaultHints;
        
        [Header("Transform Tool")]
        public UiInputHintsData transform;
        
        [Header("Transform Tool - States")]
        public UiInputHintsData transformIdle;
        public UiInputHintsData transformTransformingL;
        public UiInputHintsData transformTransformingR;
        public UiInputHintsData transformTransformingLr;
        
        [Header("Select Tool")]
        public UiInputHintsData select;

        [Header("Select Tool - States")]
        public UiInputHintsData selectIdle;
        public UiInputHintsData selectSelecting;
        public UiInputHintsData selectTransformL;
        public UiInputHintsData selectTransformR;
        public UiInputHintsData selectTransformLr;
    }
}