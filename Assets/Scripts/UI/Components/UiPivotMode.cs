using System;
using UnityEngine;
using UnityEngine.UI;
using XrInput;

namespace UI.Components
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
            Repaint();
            
            // Tooltips
            UiInputHints.AddTooltip(icons[0].gameObject, "Pivot around mesh center");
            UiInputHints.AddTooltip(icons[1].gameObject, "Pivot around hand");
            UiInputHints.AddTooltip(icons[2].gameObject, "Pivot around active selection mean");
            
            InputManager.OnActiveToolChanged += Repaint;
        }

        private void OnDestroy()
        {
            InputManager.OnActiveToolChanged -= Repaint;
        }

        public void Repaint()
        {
            _mode = InputManager.State.ActivePivotMode;
            for (var i = 0; i < icons.Length; i++)
            {
                var i1 = i;
                var btn = icons[i].GetComponent<Button>();
                btn.onClick.AddListener(() => { SetMode((PivotMode) i1); });
                if (i == PivotMode.Selection.GetHashCode())
                    btn.interactable = InputManager.State.ActiveTool != ToolType.Transform; 
                icons[i].color = _mode.GetHashCode() == i ? activeColor : Color.white;
            }
        }

        public void SetMode(PivotMode mode)
        {
            if (_mode == mode) return;

            icons[_mode.GetHashCode()].color = Color.white;
            _mode = mode;
            InputManager.State.ActivePivotMode = _mode;
            icons[_mode.GetHashCode()].color = activeColor;
        }
    }
}