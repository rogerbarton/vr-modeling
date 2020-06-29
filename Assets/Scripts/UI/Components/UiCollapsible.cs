using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Components
{
    /// <summary>
    /// UI Component header that hides items when clicked/toggled.
    /// </summary>
    public class UiCollapsible : MonoBehaviour
    {
        public TMP_Text title;
        public RectTransform checkmarkIcon;

        private readonly List<GameObject> _items = new List<GameObject>();
        private bool _visible = true;
        private int _lastSiblingIndex;

        private void OnEnable()
        {
            _lastSiblingIndex = transform.GetSiblingIndex();
        }

        /// <summary>
        /// Add an item to the group, visibility is immediately set
        /// </summary>
        /// <param name="setAsLastSibling">Add the item to the end of the group. Set this to false to have an item
        /// collapse/hide with this group but leave it in the place/sibling that it is.</param>
        public void AddItem(GameObject item, bool setAsLastSibling = true)
        {
            _items.Add(item);
            item.SetActive(_visible);
            if (setAsLastSibling)
            {
                item.transform.SetSiblingIndex(_lastSiblingIndex + 1);
                _lastSiblingIndex = item.transform.GetSiblingIndex();
            }
        }

        /// <summary>
        /// Called from UI to change the visibility of the group's items
        /// </summary>
        public void ToggleVisibility()
        {
            SetVisibility(!_visible);
        }

        /// <summary>
        /// Change the visibility of the group's items.
        /// </summary>
        public void SetVisibility(bool value)
        {
            if (value != _visible)
            {
                foreach (var item in _items)
                    item.SetActive(value);

                if (checkmarkIcon)
                    checkmarkIcon.Rotate(new Vector3(0, 0, value ? -90 : 90));
            }

            _visible = value;
        }

        /// <summary>
        /// Remove an item from the collapsible. Will not delete the item.
        /// </summary>
        public void Remove(GameObject item)
        {
            _items.Remove(item);
            _lastSiblingIndex--;
        }

        /// <summary>
        /// Remove the last item from the group.
        /// </summary>
        public void RemoveLast()
        {
            _lastSiblingIndex--;
        }
    }
}