using System;
using UnityEngine;
using UnityEngine.EventSystems;
using XrInput;

namespace UI
{
    public class UiInputHints : MonoBehaviour
    {
        public UiInputLabel title;
        public UiInputLabel help;
        public UiInputLabel trigger;
        public UiInputLabel grip;
        public UiInputLabel primaryBtn;
        public UiInputLabel secondaryBtn;
        public UiInputLabel primaryAxisX;
        public UiInputLabel primaryAxisY;

        public void SetData(UiInputHintsData data)
        {
            title.SetData(data.title);
            help.SetData(data.help);
            trigger.SetData(data.trigger);
            grip.SetData(data.grip);
            primaryBtn.SetData(data.primaryBtn);
            secondaryBtn.SetData(data.secondaryBtn);
            primaryAxisX.SetData(data.primaryAxisX);
            primaryAxisY.SetData(data.primaryAxisY);
        }

        public void SetTooltip(string data)
        {
            help.SetText(data);
        }

        public void ClearTooltip()
        {
            help.gameObject.SetActive(false);
        }

        public static void AddTooltip(GameObject uiElement, string msg)
        {
            // Hover Tooltip
            var onHoverStart = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            onHoverStart.callback.AddListener(_ =>
            {
                InputManager.get.LeftHandHints.SetTooltip(msg);
            });
            
            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                InputManager.get.LeftHandHints.ClearTooltip();
            });
            
            var trigger = uiElement.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(onHoverStart);
            trigger.triggers.Add(onHoverEnd);
        }
        
        public static void AddTooltip(GameObject uiElement, Func<string> msg)
        {
            // Hover Tooltip
            var onHoverStart = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            onHoverStart.callback.AddListener(_ =>
            {
                InputManager.get.LeftHandHints.SetTooltip(msg());
            });
            
            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                InputManager.get.LeftHandHints.ClearTooltip();
            });
            
            var trigger = uiElement.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(onHoverStart);
            trigger.triggers.Add(onHoverEnd);
        }
    }
}
