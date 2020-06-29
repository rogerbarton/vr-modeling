using UnityEngine;

namespace UI.Hints
{
    /// <summary>
    /// Data for one hand and one state of the ActiveTool. Used by the <see cref="UiInputHints"/>.
    /// We have one <see cref="UiInputLabelData"/> per button/axis.
    /// </summary>
    [CreateAssetMenu(menuName = "Data/Input Hint Data")]
    public class UiInputHintsData : ScriptableObject
    {
        public UiInputLabelData title;
        public UiInputLabelData help;
        public UiInputLabelData trigger;
        public UiInputLabelData grip;
        public UiInputLabelData primaryBtn;
        public UiInputLabelData secondaryBtn;
        public UiInputLabelData primaryAxisX;
        public UiInputLabelData primaryAxisY;
    }
}