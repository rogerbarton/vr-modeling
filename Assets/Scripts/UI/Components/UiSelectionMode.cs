using UnityEngine;
using UnityEngine.UI;
using XrInput;

namespace UI.Components
{
    /// <summary>
    /// Handles the selection mode UI, onclick behaviour
    /// There are several modes from <see cref="SelectionMode"/> as well as the <see cref="newSelectionOnDrawBtn"/>
    /// where a new selection is added on each stroke.
    /// </summary>
    public class UiSelectionMode : MonoBehaviour
    {
        public Image[] icons;
        public Color activeColor;

        public Button newSelectionOnDrawBtn;
        public Button discardSelectionOnDrawBtn;

        private SelectionMode _mode;

        public void Initialize()
        {
            _mode = InputManager.State.ActiveSelectionMode;
            for (var i = 0; i < icons.Length; i++)
            {
                var i1 = i;
                icons[i].GetComponent<Button>().onClick.AddListener(() => { SetMode((SelectionMode) i1); });
                icons[i].color = _mode.GetHashCode() == i ? activeColor : Color.white;
            }
            
            newSelectionOnDrawBtn.onClick.AddListener(ToggleNewSelectionOnDraw);
            discardSelectionOnDrawBtn.onClick.AddListener(ToggleDiscardSelectionOnDraw);
            
            // Tooltips
            UiInputHints.AddTooltip(icons[0].gameObject, "Add to selection");
            UiInputHints.AddTooltip(icons[1].gameObject, "Remove from selection");
            UiInputHints.AddTooltip(icons[2].gameObject, "Invert selection");
            UiInputHints.AddTooltip(newSelectionOnDrawBtn.gameObject, "Add each stroke into a new selection");
            UiInputHints.AddTooltip(discardSelectionOnDrawBtn.gameObject, "Clear the selection before each stroke");
        }

        public void SetMode(SelectionMode mode)
        {
            if (_mode == mode) return;

            icons[_mode.GetHashCode()].color = Color.white;
            _mode = mode;
            InputManager.State.ActiveSelectionMode = _mode;
            icons[_mode.GetHashCode()].color = activeColor;

            if (_mode != (int) SelectionMode.Add)
            {
                InputManager.State.NewSelectionOnDraw = false;
                InputManager.State.DiscardSelectionOnDraw = false;
                RepaintNewSelectionOnDrawBtn();
                RepaintDiscardSelectionOnDrawBtn();
            }
        }

        public void ToggleNewSelectionOnDraw()
        {
            if (!InputManager.State.NewSelectionOnDraw)
                SetMode((int) SelectionMode.Add);

            InputManager.State.NewSelectionOnDraw = !InputManager.State.NewSelectionOnDraw;
            InputManager.State.DiscardSelectionOnDraw = false;
            RepaintNewSelectionOnDrawBtn();
            RepaintDiscardSelectionOnDrawBtn();
        }

        private void RepaintNewSelectionOnDrawBtn()
        {
            newSelectionOnDrawBtn.image.color = InputManager.State.NewSelectionOnDraw ? activeColor : Color.white;
        }
        
        public void ToggleDiscardSelectionOnDraw()
        {
            if (!InputManager.State.DiscardSelectionOnDraw)
                SetMode((int) SelectionMode.Add);

            InputManager.State.DiscardSelectionOnDraw = !InputManager.State.DiscardSelectionOnDraw;
            InputManager.State.NewSelectionOnDraw = false;
            RepaintNewSelectionOnDrawBtn();
            RepaintDiscardSelectionOnDrawBtn();
        }

        private void RepaintDiscardSelectionOnDrawBtn()
        {
            discardSelectionOnDrawBtn.image.color = InputManager.State.DiscardSelectionOnDraw ? activeColor : Color.white;
        }
    }
}
