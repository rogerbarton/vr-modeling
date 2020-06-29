using UnityEngine;

namespace UI.Hints
{
    /// <summary>
    /// Defines the content for a <see cref="UiInputLabel"/>.
    /// </summary>
    [System.Serializable]
    public struct UiInputLabelData
    {
        public bool isOverride;
        public bool isActive;
        public Sprite icon;
        public string text;
    }
}