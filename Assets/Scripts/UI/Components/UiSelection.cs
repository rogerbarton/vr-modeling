using System.Collections.Generic;
using Libigl;
using TMPro;
using UI.Hints;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace UI.Components
{
    /// <summary>
    /// UI for one selection of a mesh (one row)
    /// </summary>
    public class UiSelection : MonoBehaviour
    {
        public TMP_Text text;
        public Button visibleBtn;
        public Button editBtn;
        public Button clearBtn;

        // Switching Icons
        [SerializeField] private Image visibleImage = null;

        [SerializeField] private Sprite visibleSprite = null;
        [SerializeField] private Sprite visibleOffSprite = null;
        
        [SerializeField] private Image editImage = null;

        [SerializeField] private Sprite editSprite = null;
        [SerializeField] private Sprite activeSprite = null;

        // References
        private int _selectionId;
        private UiCollapsible _uiCollapsible;
        private LibiglBehaviour _behaviour;
        private IList<UiSelection> _selections;

        public void Initialize(int selectionId, UiCollapsible uiCollapsible, LibiglBehaviour behaviour,
            IList<UiSelection> selections)
        {
            _behaviour = behaviour;
            _uiCollapsible = uiCollapsible;
            _selectionId = selectionId;
            _selections = selections;

            uiCollapsible.AddItem(gameObject);

            UpdateText();
            visibleBtn.image.color = Colors.Get(selectionId);

            // Behaviour when clicking buttons
            visibleBtn.onClick.AddListener(ToggleVisible);
            editBtn.onClick.AddListener(SetAsActive);
            clearBtn.onClick.AddListener(Clear);
            
            // Tooltips
            UiInputHints.AddTooltip(visibleBtn.gameObject, "Toggle visibility");
            UiInputHints.AddTooltip(editBtn.gameObject, () => _selectionId == _behaviour.Input.ActiveSelectionId ? "Active selection" : "Edit selection");
            UiInputHints.AddTooltip(clearBtn.gameObject, "Clear selection, delete if empty & last selection");

            // Apply current values
            ToggleVisibleSprite((behaviour.Input.VisibleSelectionMask & 1u << selectionId) > 0);

            if (_selections.Count > 0)
                transform.SetSiblingIndex(_selections[_selections.Count - 1].transform.GetSiblingIndex() + 1);
            _selections.Add(this);
            
            // Call once fully initialized
            SetAsActive();
        }

        #region OnClick

        public unsafe void ToggleVisible()
        {
            _behaviour.Input.VisibleSelectionMask ^= 1u << _selectionId;
            ToggleVisibleSprite((_behaviour.Input.VisibleSelectionMask & 1u << _selectionId) > 0);

            // Repaint colors if 
            if (_behaviour.State->SSize[_selectionId] > 0)
                _behaviour.Input.VisibleSelectionMaskChanged = true;
        }
        
        private void SetAsActive()
        {
            if (_selectionId == _behaviour.Input.ActiveSelectionId) return;

            // Disable the last active selection and set this one as active
            _behaviour.SetActiveSelection(_selectionId);
        }

        /// <summary>
        /// Clears the selection and deletes it if it is already empty and the last one
        /// </summary>
        public unsafe void Clear()
        {
            // Either clear the selection or delete it in the UI if it is the last one and is empty
            if (_behaviour.State->SSize[_selectionId] > 0)
                _behaviour.Input.DoClearSelection |= 1u << _selectionId;
            else if (_selectionId == _selections.Count - 1 && _selections.Count > 1)
            {
                if (_behaviour.Input.ActiveSelectionId == _selectionId)
                {
                    _behaviour.SetActiveSelection(_behaviour.Input.ActiveSelectionId -1);
                    _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(true);
                }

                _behaviour.Input.SCountUi--;
                _behaviour.Input.VisibleSelectionMask |= (uint) 1 << _selectionId;
                _behaviour.State->SSize[_selectionId] = 0;
                
                _selections.RemoveAt(_selectionId);
                _uiCollapsible.Remove(gameObject);
                Destroy(gameObject);
            }
        }

        #endregion

        public unsafe void UpdateText()
        {
            text.text = $"<b>{_selectionId}</b>: {_behaviour.State->SSize[_selectionId]} vertices";
        }

        private void ToggleVisibleSprite(bool isVisible)
        {
            visibleImage.sprite = isVisible ? visibleSprite : visibleOffSprite;
        }

        public void ToggleEditSprite(bool isActive)
        {
            editImage.sprite = isActive ? activeSprite : editSprite;
        }
    }
}
