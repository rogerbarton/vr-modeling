using UnityEngine;

namespace UI
{
    [CreateAssetMenu(menuName = "Data/Input Hint Data")]
    public class UiInputHintData : ScriptableObject
    {
        public UiInputLabelData trigger;
        public UiInputLabelData grip;
        public UiInputLabelData primaryBtn;
        public UiInputLabelData secondaryBtn;
        public UiInputLabelData primaryAxisX;
        public UiInputLabelData primaryAxisY;
    }
}