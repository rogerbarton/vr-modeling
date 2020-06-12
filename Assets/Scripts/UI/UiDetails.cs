using System;
using System.Collections.Generic;
using System.Linq;
using Libigl;
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
        private UiToggleAction _harmonicToggle;
        private UiToggleAction _arapToggle;

        public void Initialize(LibiglBehaviour behaviour)
        {
            _behaviour = behaviour;
            MeshManager.ActiveMeshSet += ActiveMeshSet;

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

            var selectionMode = Instantiate(UiManager.get.selectionModePrefab, _listParent).GetComponent<UiSelectionMode>();
            _selectionGroup.AddItem(selectionMode.gameObject);
            selectionMode.Initialize(_behaviour);
            
            _addSelectionBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _addSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
            _selectionGroup.AddItem(_addSelectionBtn.gameObject);
            _addSelectionBtn.onClick.AddListener(() => AddSelection());

            // Setup first selection
            AddSelection();
            
            var clearAllSelections = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _selectionGroup.AddItem(clearAllSelections.gameObject);
            clearAllSelections.GetComponentInChildren<TMP_Text>().text = "Clear All";
            clearAllSelections.onClick.AddListener(() =>
            {
                _behaviour.Input.DoClearSelection = uint.MaxValue;
            });
            
            
            // Operations
            _operationsGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _operationsGroup.title.text = "Operations";
            _operationsGroup.SetVisibility(true);
            
            _harmonicToggle = Instantiate(UiManager.get.toggleActionPrefab, _listParent).GetComponent<UiToggleAction>();
            _operationsGroup.AddItem(_harmonicToggle.gameObject);
            _harmonicToggle.text.text = "Harmonic";
            _harmonicToggle.button.onClick.AddListener(() => { behaviour.Input.DoHarmonicOnce = true; });
            _harmonicToggle.toggle.isOn = behaviour.Input.DoHarmonic;
            _harmonicToggle.toggle.onValueChanged.AddListener(value =>
            {
                behaviour.Input.DoHarmonicOnce = value;
                behaviour.Input.DoHarmonic = value;
            });
            
            var harmonicShowDisplacements = Instantiate(UiManager.get.togglePrefab, _listParent).GetComponent<Toggle>();
            _operationsGroup.AddItem(harmonicShowDisplacements.gameObject);
            harmonicShowDisplacements.GetComponentInChildren<TMP_Text>().text = "Toggle deform field";
            harmonicShowDisplacements.isOn = behaviour.Input.HarmonicShowDisplacement;
            harmonicShowDisplacements.onValueChanged.AddListener((value) => { behaviour.Input.HarmonicShowDisplacement = value; });
            
            _arapToggle = Instantiate(UiManager.get.toggleActionPrefab, _listParent).GetComponent<UiToggleAction>();
            _operationsGroup.AddItem(_arapToggle.gameObject);
            _arapToggle.text.text = "ARAP";
            _arapToggle.button.onClick.AddListener(() => { behaviour.Input.DoArapOnce = true; });
            _arapToggle.toggle.isOn = behaviour.Input.DoArap;
            _arapToggle.toggle.onValueChanged.AddListener(value =>
            {
                behaviour.Input.DoArapOnce = value;
                behaviour.Input.DoArap = value;
            });

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
            
            var toggleBounds = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _shaderGroup.AddItem(toggleBounds.gameObject);
            toggleBounds.GetComponentInChildren<TMP_Text>().text = "Toggle Bounds";
            toggleBounds.onClick.AddListener(() =>
            {
                behaviour.LibiglMesh.BoundingBox.gameObject.SetActive(!behaviour.LibiglMesh.BoundingBox.gameObject.activeSelf);
            });

            
            // Debug
            _debugGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Show Debug";
            _debugGroup.SetVisibility(false);
            
            var doSelectBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _debugGroup.AddItem(doSelectBtn.gameObject);
            doSelectBtn.GetComponentInChildren<TMP_Text>().text = "Do Select";
            doSelectBtn.onClick.AddListener(() => { behaviour.Input.DoSelect = true; });
            
            // Misc
            var deleteBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            deleteBtn.GetComponentInChildren<TMP_Text>().text = "Delete Mesh";
            deleteBtn.onClick.AddListener(() =>
            {
                MeshManager.get.DestroyMesh(behaviour.LibiglMesh);
            });
        }
        
        /// <returns>true if selection was successfully added, max 32 selections</returns>
        public bool AddSelection()
        {
            // Get the id and set it as the active one
            var selectionId = (int) _behaviour.Input.SCountUi;
            if (selectionId > 31) return false;
            
            _behaviour.Input.SCountUi++;

            // Add UI for the selection
            var uiSelection = Instantiate(UiManager.get.selectionPrefab, _listParent).GetComponent<UiSelection>();
            uiSelection.Initialize(selectionId, _selectionGroup, _behaviour, _selections);

            return true;
        }

        #region Callbacks

        public void OnDestroy()
        {
            MeshManager.ActiveMeshSet -= ActiveMeshSet;
        }

        /// <summary>
        /// Called in PreExecute just before the input is consumed
        /// </summary>
        public void UpdatePreExecute()
        {
            progressIcon.PreExecute();

            // Add a selection in case of the NewSelectionOnDraw
            if (_behaviour.Input.NewSelectionOnDraw &&
                _behaviour.Input.DoSelect && !_behaviour.Input.DoSelectPrev && // Just started stroke 
                _behaviour.State->SSize[_behaviour.Input.ActiveSelectionId] > 0) // Active selection not empty
                AddSelection();

            // Copy UI selection count to the state (as it is shared with C++)
            _behaviour.State->SCount = _behaviour.Input.SCountUi;
        }

        /// <summary>
        /// Update the UI Details panel after executing
        /// </summary>
        public void UpdatePostExecute()
        {
            // Update Selection UI
            if (_behaviour.State->DirtySelectionsResized > 0)
            {
                UpdateVertexCountText();
                for (var i = 0; i < _selections.Count; i++)
                {
                    if ((_behaviour.State->DirtySelectionsResized & 1 << i) > 0)
                        _selections[i].UpdateText();
                }
            }

            progressIcon.PostExecute();
        }

        /// <summary>
        /// Called when the active mesh changes
        /// </summary>
        private void ActiveMeshSet()
        {
            var isActive = _behaviour.LibiglMesh.IsActiveMesh();
            activeImage.sprite = isActive ? activeSprite : editSprite;
        }

        #endregion

        #region Helper Functions

        private void UpdateVertexCountText()
        {
            _vertexCount.text =
                $"<b>V</b>: {_behaviour.State->SSizeAll}/{_behaviour.State->VSize} <b>F</b>: {_behaviour.State->FSize}";
        }

        #endregion
    }
}
