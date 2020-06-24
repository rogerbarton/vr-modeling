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

        [SerializeField] private UiInputHintsDataCollection collection;
        private UiInputHintsData _currentData;


        public void SetData(UiInputHintsData data)
        {
            _currentData = data;
            title.SetData(data.title);
            help.SetData(data.help);
            trigger.SetData(data.trigger);
            grip.SetData(data.grip);
            primaryBtn.SetData(data.primaryBtn);
            secondaryBtn.SetData(data.secondaryBtn);
            primaryAxisX.SetData(data.primaryAxisX);
            primaryAxisY.SetData(data.primaryAxisY);
        }

        public void SetTooltip(string data, bool overrideExisting = false)
        {
            if(!_currentData.help.isActive || overrideExisting)
                help.SetData(new UiInputLabelData{isActive = true, text = data, icon = Icons.get.help});
        }

        public void ClearTooltip()
        {
            help.SetData(_currentData.help);
        }

        public void ResetData()
        {
            SetData(_currentData);
        }
        
        /// <summary>
        /// Display the message when hovering over this UI element.
        /// Note: Must be attached to the gameObject with the button/toggle component, otherwise clicking may be blocked
        /// </summary>
        /// <param name="uiElement">GameObject with the Button/Toggle UI component</param>
        /// <param name="msg">Message to display when hovering</param>
        public static void AddTooltip(GameObject uiElement, string msg)
        {
            // Hover Tooltip
            var onHoverStart = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            onHoverStart.callback.AddListener(_ =>
            {
                InputManager.get.HandHintsL?.SetTooltip(msg);
            });
            
            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                InputManager.get.HandHintsL?.ClearTooltip();
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
                InputManager.get.HandHintsL?.SetTooltip(msg());
            });
            
            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                InputManager.get.HandHintsL?.ClearTooltip();
            });
            
            var trigger = uiElement.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(onHoverStart);
            trigger.triggers.Add(onHoverEnd);
        }
        
        
        
        /// <summary>
        /// Refreshes the Input hints based on the <see cref="ActiveTool"/> and the sub state like <see cref="ToolTransformMode"/>
        /// </summary>
        public void Repaint()
        {
            SetData(collection.defaultHints);
            switch (InputManager.State.ActiveTool)
            {
                case ToolType.Transform:
                    SetData(collection.transform);
                    switch (InputManager.State.ToolTransformMode)
                    {
                        case ToolTransformMode.Idle:
                            SetData(collection.transformIdle);
                            break;
                        case ToolTransformMode.TransformingL:
                            SetData(collection.transformTransformingL);
                            break;
                        case ToolTransformMode.TransformingR:
                            SetData(collection.transformTransformingR);
                            break;
                        case ToolTransformMode.TransformingLR:
                            SetData(collection.transformTransformingLr);
                            break;
                    }

                    break;
                case ToolType.Select:
                    SetData(collection.select);
                    switch (InputManager.State.ToolSelectMode)
                    {
                        case ToolSelectMode.Idle:
                            SetData(collection.selectIdle);
                            break;
                        case ToolSelectMode.Selecting:
                            SetData(collection.selectSelecting);
                            break;
                        case ToolSelectMode.TransformingL:
                            SetData(collection.selectTransformL);
                            break;
                        case ToolSelectMode.TransformingR:
                            SetData(collection.selectTransformR);
                            break;
                        case ToolSelectMode.TransformingLR:
                            SetData(collection.selectTransformLr);
                            break;
                    }
                    break;
            }
        }
    }
}
