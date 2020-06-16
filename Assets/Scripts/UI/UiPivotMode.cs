﻿using UnityEngine;
using UnityEngine.UI;
using XrInput;

namespace UI
{
    /// <summary>
    /// Similar to UiSelectionMode, effectively an enum field
    /// </summary>
    public class UiPivotMode : MonoBehaviour
    {
        public Image[] icons;
        public Color activeColor;

        private PivotMode _mode;
        
        public void Initialize()
        {
            _mode = InputManager.Input.ActivePivotMode;
            for (var i = 0; i < icons.Length; i++)
            {
                var i1 = i;
                icons[i].GetComponent<Button>().onClick.AddListener(() => { SetMode((PivotMode) i1); });
                icons[i].color = _mode.GetHashCode() == i ? activeColor : Color.white;
            }
        }

        public void SetMode(PivotMode mode)
        {
            if (_mode == mode) return;

            icons[_mode.GetHashCode()].color = Color.white;
            _mode = mode;
            InputManager.Input.ActivePivotMode = _mode;
            icons[_mode.GetHashCode()].color = activeColor;
        }
    }
}