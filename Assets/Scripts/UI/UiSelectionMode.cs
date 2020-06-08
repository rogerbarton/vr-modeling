using libigl.Behaviour;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles the selection mode UI, onclick behaviour
    /// There are several modes from <see cref="SelectionMode"/> as well as the <see cref="newSelectionOnDrawBtn"/>
    /// where a new selection is added on each stroke.
    /// </summary>
    public class UiSelectionMode : MonoBehaviour
    {
        public Image[] images;
        public Color activeColor;

        public Button newSelectionOnDrawBtn;

        private int _modeId = (int) SelectionMode.Add;
        private LibiglBehaviour _behaviour;
        public void Initialize(LibiglBehaviour behaviour)
        {
            _behaviour = behaviour;
            _modeId = _behaviour.Input.ActiveSelectionId.GetHashCode();
            for (var i = 0; i < images.Length; i++)
            {
                var i1 = i;
                images[i].GetComponent<Button>().onClick.AddListener(() => { SetMode(i1); });
                images[i].color = _modeId == i ? activeColor : Color.white;
            }
            
            newSelectionOnDrawBtn.onClick.AddListener(ToggleNewSelectionOnDraw);
        }

        public void SetMode(int selectionMode)
        {
            if (_modeId == selectionMode) return;

            images[_modeId].color = Color.white;
            _modeId = selectionMode;
            _behaviour.Input.ActiveSelectionMode = (SelectionMode) _modeId;
            images[_modeId].color = activeColor;

            if (_modeId != (int) SelectionMode.Add)
            {
                _behaviour.Input.NewSelectionOnDraw = false;
                RepaintNewSelectionOnDrawBtn();
            }
        }

        public void ToggleNewSelectionOnDraw()
        {
            if (!_behaviour.Input.NewSelectionOnDraw)
                SetMode((int) SelectionMode.Add);

            _behaviour.Input.NewSelectionOnDraw = !_behaviour.Input.NewSelectionOnDraw;
            RepaintNewSelectionOnDrawBtn();
        }

        private void RepaintNewSelectionOnDrawBtn()
        {
            newSelectionOnDrawBtn.image.color = _behaviour.Input.NewSelectionOnDraw ? activeColor : Color.white;
        }
    }
}
