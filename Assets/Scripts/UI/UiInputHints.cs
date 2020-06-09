using TMPro;
using UnityEngine;

namespace UI
{
    public class UiInputHints : MonoBehaviour
    {
        public UiInputLabel trigger;
        public UiInputLabel grip;
        public UiInputLabel primaryBtn;
        public UiInputLabel secondaryBtn;
        public UiInputLabel primaryAxisX;
        public UiInputLabel primaryAxisY;

        public void SetData(UiInputHintData data)
        {
            trigger.SetData(data.trigger);
            grip.SetData(data.grip);
            primaryBtn.SetData(data.primaryBtn);
            secondaryBtn.SetData(data.secondaryBtn);
            primaryAxisX.SetData(data.primaryAxisX);
            primaryAxisY.SetData(data.primaryAxisY);
        }
    }
}
