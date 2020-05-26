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

        // UI Components
        private TMP_Text _meshName;
        private TMP_Text _vertexCount;
        private TMP_Text _faceCount;
        private TMP_Text _selectCount;

        private Button _setActiveBtn;
        private Button _resetTransformBtn;

        private Button _toggleWireframe;

        private UiCollapsible _selectionGroup;
        private Button _addSelectionBtn;
        private readonly List<UiSelection> _selections = new List<UiSelection>();

        public void Initialize(LibiglBehaviour behaviour)
        {
            _behaviour = behaviour;
            MeshManager.ActiveMeshChanged += ActiveMeshChanged;

            _listParent = GetComponentInChildren<VerticalLayoutGroup>().transform;

            _meshName = Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
            _meshName.text = _behaviour.LibiglMesh.name;

            _vertexCount = Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
            _vertexCount.text = $"Vertices: {_behaviour.State->VSize}";

            _faceCount = Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
            _faceCount.text = $"Faces: {_behaviour.State->FSize}";

            _selectCount = Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
            _selectCount.text = $"Selected: {_behaviour.State->SSizeAll}";

            // Set mesh as the active one if not active already
            _setActiveBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _setActiveBtn.GetComponentInChildren<TMP_Text>().text = "Set As Active";
            _setActiveBtn.onClick.AddListener(() => { MeshManager.SetActiveMesh(_behaviour.LibiglMesh); });
            _setActiveBtn.gameObject.SetActive(!_behaviour.LibiglMesh.IsActiveMesh());

            // Reset Transform to spawnPoint button 
            _resetTransformBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _resetTransformBtn.GetComponentInChildren<TMP_Text>().text = "Reset Transform";
            _resetTransformBtn.onClick.AddListener(() => { behaviour.LibiglMesh.ResetTransformToSpawn(); });

            // Shaders
            // var ShaderHeader = Object.Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
            // ShaderHeader.text = "Shader";

            _toggleWireframe = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
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

            // Selection
            _selectionGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _selectionGroup.title.text = "Selections";
            _selectionGroup.visible = true;

            _addSelectionBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _addSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
            _selectionGroup.GetComponent<UiCollapsible>().AddItem(_addSelectionBtn.gameObject);
            _addSelectionBtn.onClick.AddListener(AddSelection);

            // Setup first selection
            AddSelection();
            // Set it as the active one
            _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(true);
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
            _selectionGroup.GetComponent<UiCollapsible>().AddItem(uiSelection.gameObject);
            uiSelection.visibleBtn.image.color = Util.Colors.GetColorById(selectionId);

            // Behaviour when clicking buttons
            uiSelection.visibleBtn.onClick.AddListener(() =>
            {
                _behaviour.Input.VisibleSelectionMask ^= 1u << selectionId;
                uiSelection.ToggleVisibleSprite((_behaviour.Input.VisibleSelectionMask & 1u << selectionId) > 0);
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
                _behaviour.Input.DoClearSelection |= 1u << selectionId;
            });

            _selections.Add(uiSelection);

            // Set as active, TODO: extract shared function with onClick editBtn
            _selections[_behaviour.Input.ActiveSelectionId].ToggleEditSprite(false);
            _behaviour.Input.ActiveSelectionId = selectionId;
            uiSelection.ToggleEditSprite(true);
        }

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
            if (_behaviour.Input.DoSelect)
            {
                _selectCount.text = $"Selected: {_behaviour.State->SSizeAll}";
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
            _setActiveBtn.gameObject.SetActive(!_behaviour.LibiglMesh.IsActiveMesh());
        }

        public void ActiveSelectionChanged()
        {
            // TODO: Update UI that shows active selection
        }
    }
}
