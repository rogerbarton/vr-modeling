using System;
using Libigl;
using UnityEngine;
using UnityEngine.EventSystems;
using Util;
using XrInput;

namespace UI.Hints
{
    /// <summary>
    /// Defines the behaviour of the input hints of one hand.
    /// Important functions are: <see cref="AddTooltip(GameObject, string)"/>, <see cref="SetData"/> and <see cref="Repaint"/>.<p/>
    /// In short, tooltips can be added so when we hover over a UI element it displays some text.
    /// Repaint is called with the collection to set out the default hints for a particular state.
    /// This can then be overriden by scripting, e.g. see <see cref="RepaintTriggerColor"/> where we set the
    /// trigger hint color ot the active selection color.
    /// </summary>
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

        [SerializeField] private UiInputHintsDataCollection collection = null;
        private UiInputHintsData _currentData;

        private LibiglBehaviour _activeBehaviour;

        public void Initialize()
        {
            title.Initialize();
            help.Initialize();
            trigger.Initialize();
            grip.Initialize();
            primaryBtn.Initialize();
            secondaryBtn.Initialize();
            primaryAxisX.Initialize();
            primaryAxisY.Initialize();

            _activeBehaviour = MeshManager.ActiveMesh.Behaviour;
            _activeBehaviour.OnActiveSelectionChanged += RepaintTriggerColor;
            RepaintTriggerColor();

            MeshManager.OnActiveMeshChanged += OnActiveMeshChanged;
        }

        /// <summary>
        /// Set the data that should be displayed.
        /// Note: Consider the overrides and the ordering when calling this multiple times. 
        /// </summary>
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
                        case ToolTransformMode.TransformingLr:
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
                            RepaintTriggerColor();
                            break;
                        case ToolSelectMode.Selecting:
                            SetData(collection.selectSelecting);
                            RepaintTriggerColor();
                            break;
                        case ToolSelectMode.TransformingL:
                            SetData(collection.selectTransformL);
                            break;
                        case ToolSelectMode.TransformingR:
                            SetData(collection.selectTransformR);
                            break;
                        case ToolSelectMode.TransformingLr:
                            SetData(collection.selectTransformLr);
                            break;
                    }

                    break;
            }
        }

        #region Custom Functionality

        #region - Tooltips

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
                if (InputManager.get.HandHintsL)
                    InputManager.get.HandHintsL.SetTooltip(msg, true);
            });

            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                if (InputManager.get.HandHintsL)
                    InputManager.get.HandHintsL.ClearTooltip();
            });

            var trigger = uiElement.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(onHoverStart);
            trigger.triggers.Add(onHoverEnd);
        }

        /// <summary>
        /// An overload where the message can be a lambda.
        /// </summary>
        public static void AddTooltip(GameObject uiElement, Func<string> msg)
        {
            // Hover Tooltip
            var onHoverStart = new EventTrigger.Entry {eventID = EventTriggerType.PointerEnter};
            onHoverStart.callback.AddListener(_ =>
            {
                if (InputManager.get.HandHintsL)
                    InputManager.get.HandHintsL.SetTooltip(msg(), true);
            });

            var onHoverEnd = new EventTrigger.Entry {eventID = EventTriggerType.PointerExit};
            onHoverEnd.callback.AddListener(_ =>
            {
                if (InputManager.get.HandHintsL)
                    InputManager.get.HandHintsL.ClearTooltip();
            });

            var trigger = uiElement.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Add(onHoverStart);
            trigger.triggers.Add(onHoverEnd);
        }

        /// <summary>
        /// Apply text to the help/tooltip. Used by the UI.
        /// </summary>
        private void SetTooltip(string data, bool overrideExisting = false)
        {
            if (!_currentData.help.isActive || overrideExisting)
                help.SetData(
                    new UiInputLabelData {isOverride = true, isActive = true, text = data, icon = Icons.get.help},
                    false);
        }

        private void ClearTooltip()
        {
            help.ResetToData();
        }

        #endregion

        #region - Trigger Selection Color

        /// <summary>
        /// Sets the trigger hint background color based on the active selection for the active mesh.
        /// Implementation note: Example of input hints driven by scripting. We ensure with the events On*Changed that
        /// this function is called only
        /// when the ActiveMesh changes or the selection of the ActiveMesh.
        /// </summary>
        private void RepaintTriggerColor()
        {
            if (InputManager.State.ActiveTool != ToolType.Select) return;
            var selectionId = MeshManager.ActiveMesh.Behaviour.Input.ActiveSelectionId;
            trigger.SetColor(Colors.Get(selectionId));
        }

        private void OnActiveMeshChanged()
        {
            // Update the RepaintTriggerColor to only be called for the ActiveMesh
            _activeBehaviour.OnActiveSelectionChanged -= RepaintTriggerColor;
            _activeBehaviour = MeshManager.ActiveMesh.Behaviour;
            _activeBehaviour.OnActiveSelectionChanged += RepaintTriggerColor;
            RepaintTriggerColor();
        }

        #endregion

        #endregion

        private void OnDestroy()
        {
            _activeBehaviour.OnActiveSelectionChanged -= RepaintTriggerColor;
            MeshManager.OnActiveMeshChanged -= OnActiveMeshChanged;
        }
    }
}