using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Hides items when clicked/toggled
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
        public void AddItem(GameObject item)
        {
            _items.Add(item);
            item.SetActive(_visible);
            item.transform.SetSiblingIndex(_lastSiblingIndex + 1);
            _lastSiblingIndex = item.transform.GetSiblingIndex();
        }

        public void RemoveLast()
        {
            _lastSiblingIndex--;
        }

        public void ToggleVisibility()
        {
            SetVisibility(!_visible);
        }

        /// <summary>
        /// Change the visibility, callable from UI Event OnClick
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

        public void Remove(GameObject go)
        {
            _items.Remove(go);
            _lastSiblingIndex--;
        }
    }
}
