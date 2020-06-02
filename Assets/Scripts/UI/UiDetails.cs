using System.Collections.Generic;
using System.Linq;
using libigl;
using libigl.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public unsafe class UiDetails : MonoBehaviour
    {
        private LibiglBehaviour _behaviour;
        private Transform _listParent;
        public UiProgressIcon progressIcon;

        public Button activeBtn;
        [SerializeField] private Image activeImage = null;
        [SerializeField] private Sprite editSprite = null;
        [SerializeField] private Sprite activeSprite = null;

        // UI Components
        private TMP_Text _meshName;
        private TMP_Text _vertexCount;

        private Button _toggleWireframe;

        private UiCollapsible _selectionGroup;
        private Button _addSelectionBtn;
        private readonly List<UiSelection> _selections = new List<UiSelection>();
        private UiCollapsible _operationsGroup;
        private UiCollapsible _shaderGroup;
        private UiCollapsible _debugGroup;
        private Button _harmonicToggle;
        private Button _arapToggle;

        public void Initialize(LibiglBehaviour behaviour)
        {
            _behaviour = behaviour;
            MeshManager.ActiveMeshChanged += ActiveMeshChanged;

            activeBtn.onClick.AddListener(() => { MeshManager.SetActiveMesh(_behaviour.LibiglMesh); });
            var isActive = _behaviour.LibiglMesh.IsActiveMesh();
            activeImage.sprite = isActive ? activeSprite : editSprite;
            
            _listParent = GetComponentInChildren<VerticalLayoutGroup>().transform;

            // Start UI Generation
            _meshName = Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
            _meshName.text = _behaviour.LibiglMesh.name;

            _vertexCount = Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
            UpdateVertexCountText();

            
            // Selection
            _selectionGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _selectionGroup.title.text = "Selections";
            _selectionGroup.SetVisibility(true);

            _addSelectionBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _addSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
            _selectionGroup.AddItem(_addSelectionBtn.gameObject);
            _addSelectionBtn.onClick.AddListener(AddSelection);

            // Setup first selection
            AddSelection();
            // Set it as the active one
            _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(true);
            
            
            // Operations
            _operationsGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _operationsGroup.title.text = "Operations";
            _operationsGroup.SetVisibility(true);
            
            _harmonicToggle = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _operationsGroup.AddItem(_harmonicToggle.gameObject);
            _harmonicToggle.GetComponentInChildren<TMP_Text>().text = "Harmonic";
            _harmonicToggle.onClick.AddListener(() => { behaviour.Input.DoHarmonic = true; });
            
            _arapToggle = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _operationsGroup.AddItem(_arapToggle.gameObject);
            _arapToggle.GetComponentInChildren<TMP_Text>().text = "ARAP";
            // _arapToggle.onClick.AddListener(() => { behaviour.Input.DoArap = true; });

            var resetTransformBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _operationsGroup.AddItem(resetTransformBtn.gameObject);
            resetTransformBtn.GetComponentInChildren<TMP_Text>().text = "Reset Transform";
            resetTransformBtn.onClick.AddListener(() => { behaviour.LibiglMesh.ResetTransformToSpawn(); });
            
            
            // Shaders
            _shaderGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _shaderGroup.title.text = "Selections";
            _shaderGroup.SetVisibility(true);

            _toggleWireframe = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _shaderGroup.AddItem(_toggleWireframe.gameObject);
            _shaderGroup.AddItem(_toggleWireframe.gameObject);
            _toggleWireframe.GetComponentInChildren<TMP_Text>().text = "Toggle Wireframe";
            _toggleWireframe.onClick.AddListener(() =>
            {
                var materials = behaviour.LibiglMesh.MeshRenderer.materials;
                if (materials.Length == 1)
                    materials = materials.Append(MeshManager.get.wireframeMaterial).ToArray();
                else
                    materials = new[] {materials.First()};
                behaviour.LibiglMesh.MeshRenderer.materials = materials;
            });

            
            // Debug
            _debugGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Show Debug";
            _debugGroup.SetVisibility(false);
            
            var doSelectBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _debugGroup.AddItem(doSelectBtn.gameObject);
            doSelectBtn.GetComponentInChildren<TMP_Text>().text = "Do Select";
            doSelectBtn.onClick.AddListener(() => { behaviour.Input.DoSelect = true; });
        }

        private void AddSelection()
        {
            // Get the id and set it as the active one
            var selectionId = (int) _behaviour.Input.SCount;
            _behaviour.Input.SCount++;

            // Add UI for the selection
            var uiSelection = Instantiate(UiManager.get.selectionPrefab, _listParent)
                .GetComponent<UiSelection>();
            uiSelection.text.text = $"<b>{selectionId}</b>: 0 vertices";
            _selectionGroup.AddItem(uiSelection.gameObject);
            uiSelection.visibleBtn.image.color = Util.Colors.GetColorById(selectionId);

            // Behaviour when clicking buttons
            uiSelection.visibleBtn.onClick.AddListener(() =>
            {
                _behaviour.Input.VisibleSelectionMask ^= 1u << selectionId;
                uiSelection.ToggleVisibleSprite((_behaviour.Input.VisibleSelectionMask & 1u << selectionId) > 0);

                // Repaint colors if 
                if (_behaviour.State->SSize[selectionId] > 0)
                    _behaviour.Input.VisibleSelectionMaskChanged = true;
            });
            uiSelection.editBtn.onClick.AddListener(() =>
            {
                if (selectionId == _behaviour.Input.ActiveSelectionId) return;

                // Disable the last active selection and set this one as active
                _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(false);
                _behaviour.Input.ActiveSelectionId = selectionId;
                uiSelection.ToggleEditSprite(true);
            });
            uiSelection.clearBtn.onClick.AddListener(() =>
            {
                // Either clear the selection or delete it in the UI if it is the last one and is empty
                if(_behaviour.State->SSize[selectionId] > 0)
                    _behaviour.Input.DoClearSelection |= 1u << selectionId;
                else if (selectionId == _selections.Count -1 && _selections.Count > 1)
                {
                    _selections.RemoveAt(selectionId);
                    Destroy(_selections[selectionId].gameObject);
                }
            });

            _selections.Add(uiSelection);

            // Apply current values
            uiSelection.ToggleVisibleSprite((_behaviour.Input.VisibleSelectionMask & 1u << selectionId) > 0);

            // Set as active, TODO: extract shared function with onClick editBtn
            _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(false);
            _behaviour.Input.ActiveSelectionId = selectionId;
            uiSelection.ToggleEditSprite(true);

        }

        #region Callbacks

        public void OnDestroy()
        {
            MeshManager.ActiveMeshChanged -= ActiveMeshChanged;
        }

        /// <summary>
        /// Called in PreExecute just before the input is consumed
        /// </summary>
        public void UpdatePreExecute()
        {
            progressIcon.PreExecute();
        }

        /// <summary>
        /// Update the UI Details panel after executing
        /// </summary>
        public void UpdatePostExecute()
        {
            // Update Selection UI
            if (_behaviour.State->DirtySelections > 0)
            {
                UpdateVertexCountText();
                for (var i = 0; i < _selections.Count; i++)
                    _selections[i].GetComponentInChildren<TMP_Text>().text =
                        $"{i}: {_behaviour.State->SSize[i]} vertices";
            }

            progressIcon.PostExecute();
        }

        /// <summary>
        /// Called when the active mesh changes
        /// </summary>
        private void ActiveMeshChanged()
        {
            var isActive = _behaviour.LibiglMesh.IsActiveMesh();
            activeImage.sprite = isActive ? activeSprite : editSprite;
        }

        #endregion

        #region Helper Functions

        private void UpdateVertexCountText()
        {
            _vertexCount.text =
                $"V: {_behaviour.State->SSizeAll}/{_behaviour.State->VSize} F: {_behaviour.State->FSize}";
        }

        #endregion
    }
}
