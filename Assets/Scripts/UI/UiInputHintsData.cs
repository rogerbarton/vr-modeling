using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "Data/Input Hint Data")]
    public class UiInputHintsData : ScriptableObject
    {
        public UiInputLabelData title;
        public UiInputLabelData trigger;
        public UiInputLabelData grip;
        public UiInputLabelData primaryBtn;
        public UiInputLabelData secondaryBtn;
        public UiInputLabelData primaryAxisX;
        public UiInputLabelData primaryAxisY;
    }
}