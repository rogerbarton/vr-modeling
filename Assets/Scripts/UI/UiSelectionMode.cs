using libigl.Behaviour;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class UiSelectionMode : MonoBehaviour
    {
        public Image[] images;
        public Color activeColor;

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
        }

        public void SetMode(int selectionMode)
        {
            if (_modeId == selectionMode) return;

            images[_modeId].color = Color.white;
            _modeId = selectionMode;
            _behaviour.Input.ActiveSelectionMode = (SelectionMode) _modeId;
            images[_modeId].color = activeColor;
        }
    }
}
