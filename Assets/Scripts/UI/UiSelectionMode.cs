using Libigl;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using XrInput;

namespace UI
{
    /// <summary>
    /// Handles the selection mode UI, onclick behaviour
    /// There are several modes from <see cref="SelectionMode"/> as well as the <see cref="newSelectionOnDrawBtn"/>
    /// where a new selection is added on each stroke.
    /// </summary>
    public class UiSelectionMode : MonoBehaviour
    {
        [FormerlySerializedAs("images")] public Image[] icons;
        public Color activeColor;

        public Button newSelectionOnDrawBtn;

        private SelectionMode _modeId = (int) SelectionMode.Add;

        public void Initialize()
        {
            _modeId = InputManager.Input.ActiveSelectionMode;
            for (var i = 0; i < icons.Length; i++)
            {
                var i1 = i;
                icons[i].GetComponent<Button>().onClick.AddListener(() => { SetMode((SelectionMode) i1); });
                icons[i].color = _modeId.GetHashCode() == i ? activeColor : Color.white;
            }
            
            newSelectionOnDrawBtn.onClick.AddListener(ToggleNewSelectionOnDraw);
        }

        public void SetMode(SelectionMode selectionMode)
        {
            if (_modeId == selectionMode) return;

            icons[_modeId.GetHashCode()].color = Color.white;
            _modeId = selectionMode;
            InputManager.Input.ActiveSelectionMode = (SelectionMode) _modeId;
            icons[_modeId.GetHashCode()].color = activeColor;

            if (_modeId != (int) SelectionMode.Add)
            {
                InputManager.Input.NewSelectionOnDraw = false;
                RepaintNewSelectionOnDrawBtn();
            }
        }

        public void ToggleNewSelectionOnDraw()
        {
            if (!InputManager.Input.NewSelectionOnDraw)
                SetMode((int) SelectionMode.Add);

            InputManager.Input.NewSelectionOnDraw = !InputManager.Input.NewSelectionOnDraw;
            RepaintNewSelectionOnDrawBtn();
        }

        private void RepaintNewSelectionOnDrawBtn()
        {
            newSelectionOnDrawBtn.image.color = InputManager.Input.NewSelectionOnDraw ? activeColor : Color.white;
        }
    }
}
