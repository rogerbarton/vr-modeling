using System.Collections.Generic;
using Libigl;
using TMPro;
using UI.Components;
using UI.Hints;
using UnityEngine;
using UnityEngine.UI;
using XrInput;

namespace UI
{
    /// <summary>
    /// Contains all functionality related to the mesh UI panel.
    /// There is one of these per LibiglMesh.
    /// Contains most UI generation.
    /// </summary>
    public unsafe class UiMeshDetails : MonoBehaviour
    {
        /// <summary>
        /// Access to the behaviour that this UI panel belongs to
        /// </summary>
        private LibiglBehaviour _behaviour;
        
        /// <summary>
        /// The content of the scroll list view, add new components as a child of this transform.
        /// </summary>
        private Transform _listParent;

        public UiProgressIcon progressIcon;
        public Button activeBtn;
        public Button deleteBtn;
        [SerializeField] private Image activeImage = null;
        [SerializeField] private Sprite editSprite = null;
        [SerializeField] private Sprite activeSprite = null;
        [SerializeField] private Image background = null;
        [Tooltip("The background color of the UI Panel it is the ActiveMesh.")]
        [SerializeField] private Color activeBackgroundColor = Color.white;
        private Color _defaultBackgroundColor;

        // -- UI Component Instances
        private TMP_Text _vertexCount;
        private UiCollapsible _selectionGroup;
        private readonly List<UiSelection> _selections = new List<UiSelection>();

        /// <summary>
        /// The active selection displayed in the UI
        /// </summary>
        private int _activeSelectionId = -1;

        private UiCollapsible _debugGroup;
        private UiToggleAction _harmonicToggle;
        private UiToggleAction _arapToggle;

        /// <summary>
        /// Main function where UI is generated.
        /// </summary>
        /// <param name="behaviour">The behaviour we are generating the UI panel for.</param>
        public void Initialize(LibiglBehaviour behaviour)
        {
            _behaviour = behaviour;
            MeshManager.OnActiveMeshChanged += RepaintActiveMesh;
            _defaultBackgroundColor = background.color;

            // -- Existing UI in Prefab
            activeBtn.onClick.AddListener(() => { MeshManager.SetActiveMesh(_behaviour.Mesh); });
            var isActive = _behaviour.Mesh.IsActiveMesh();
            activeImage.sprite = isActive ? activeSprite : editSprite;
            UiInputHints.AddTooltip(activeBtn.gameObject,
                () => _behaviour.Mesh.IsActiveMesh() ? "This is the active mesh" : "Make this mesh active");

            deleteBtn.onClick.AddListener(() => { MeshManager.get.DestroyMesh(behaviour.Mesh); });

            _listParent = GetComponentInChildren<VerticalLayoutGroup>().transform;

            // -- Start UI Generation
            var meshName = Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
            meshName.text = _behaviour.Mesh.name;

            _vertexCount = Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
            UpdateVertexCountText();


            // -- Selection
            _selectionGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _selectionGroup.title.text = "Selections";
            _selectionGroup.SetVisibility(true);

            var addSelectionBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            addSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
            _selectionGroup.AddItem(addSelectionBtn.gameObject);
            addSelectionBtn.onClick.AddListener(() => AddSelection());

            // Setup first selection
            behaviour.OnActiveSelectionChanged += RepaintActiveSelection;
            AddSelection();

            var clearAllSelections = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            _selectionGroup.AddItem(clearAllSelections.gameObject);
            clearAllSelections.GetComponentInChildren<TMP_Text>().text = "Clear All";
            clearAllSelections.onClick.AddListener(() =>
            {
                // Clear and remove all but the first selection
                _behaviour.SetActiveSelection(0);
                _behaviour.Input.DoClearSelection = uint.MaxValue;
                for (var i = _selections.Count -1; i > 0; i--)
                    _selections[i].Clear(true);
            });


            // -- Operations
            var operationsGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            operationsGroup.title.text = "Operations";
            operationsGroup.SetVisibility(true);

            _harmonicToggle = Instantiate(UiManager.get.toggleActionPrefab, _listParent).GetComponent<UiToggleAction>();
            operationsGroup.AddItem(_harmonicToggle.gameObject);
            _harmonicToggle.text.text = "Harmonic";
            _harmonicToggle.button.onClick.AddListener(() => { behaviour.Input.DoHarmonic = true; });
            _harmonicToggle.toggle.isOn = behaviour.Input.DoHarmonicRepeat;
            UiInputHints.AddTooltip(_harmonicToggle.button.gameObject, "Run harmonic once");
            UiInputHints.AddTooltip(_harmonicToggle.toggle.gameObject, "Run harmonic continuously");
            _harmonicToggle.toggle.onValueChanged.AddListener(value =>
            {
                behaviour.Input.DoHarmonic = value;
                behaviour.Input.DoHarmonicRepeat = value;
                if (value && behaviour.Input.DoArapRepeat)
                {
                    behaviour.Input.DoArap = false;
                    behaviour.Input.DoArapRepeat = false;
                    _arapToggle.toggle.isOn = false;
                }
            });

            var harmonicShowDisplacements = Instantiate(UiManager.get.togglePrefab, _listParent).GetComponent<Toggle>();
            operationsGroup.AddItem(harmonicShowDisplacements.gameObject);
            harmonicShowDisplacements.GetComponentInChildren<TMP_Text>().text = "Toggle deform field";
            harmonicShowDisplacements.isOn = behaviour.Input.HarmonicShowDisplacement;
            harmonicShowDisplacements.onValueChanged.AddListener((value) =>
            {
                behaviour.Input.HarmonicShowDisplacement = value;
            });

            _arapToggle = Instantiate(UiManager.get.toggleActionPrefab, _listParent).GetComponent<UiToggleAction>();
            operationsGroup.AddItem(_arapToggle.gameObject);
            _arapToggle.text.text = "ARAP";
            _arapToggle.button.onClick.AddListener(() => { behaviour.Input.DoArap = true; });
            _arapToggle.toggle.isOn = behaviour.Input.DoArapRepeat;
            UiInputHints.AddTooltip(_arapToggle.button.gameObject, "Run As-Rigid-As-Possible once");
            UiInputHints.AddTooltip(_arapToggle.toggle.gameObject, "Run As-Rigid-As-Possible continuously");
            _arapToggle.toggle.onValueChanged.AddListener(value =>
            {
                behaviour.Input.DoArap = value;
                behaviour.Input.DoArapRepeat = value;
                if (value && behaviour.Input.DoHarmonicRepeat)
                {
                    behaviour.Input.DoHarmonic = false;
                    behaviour.Input.DoHarmonicRepeat = false;
                    _harmonicToggle.toggle.isOn = false;
                }
            });

            var resetMeshBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            operationsGroup.AddItem(resetMeshBtn.gameObject);
            resetMeshBtn.GetComponentInChildren<TMP_Text>().text = "Reset Mesh Vertices";
            resetMeshBtn.onClick.AddListener(() => { behaviour.Input.ResetV = true; });

            var resetTransformBtn = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            operationsGroup.AddItem(resetTransformBtn.gameObject);
            resetTransformBtn.GetComponentInChildren<TMP_Text>().text = "Reset Transform";
            resetTransformBtn.onClick.AddListener(() => { behaviour.Mesh.ResetTransformToSpawn(); });


            // -- Shaders
            var shaderGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            shaderGroup.title.text = "Selections";
            shaderGroup.SetVisibility(true);

            var toggleWireframe = Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
            shaderGroup.AddItem(toggleWireframe.gameObject);
            toggleWireframe.GetComponentInChildren<TMP_Text>().text = "Toggle Wireframe";
            toggleWireframe.onClick.AddListener(() => { _behaviour.Mesh.ToggleWireframe(); });


            // -- Debug
            _debugGroup = Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Show Debug";
            _debugGroup.SetVisibility(false);

            // Call when constructed as we likely just missed this
            RepaintActiveMesh();
        }

        #region Callbacks

        public void OnDestroy()
        {
            _behaviour.OnActiveSelectionChanged -= RepaintActiveSelection;
            MeshManager.OnActiveMeshChanged -= RepaintActiveMesh;
        }

        /// <summary>
        /// Called in PreExecute just before the input is consumed
        /// </summary>
        public void UpdatePreExecute()
        {
            progressIcon.PreExecute();

            // Add a selection in case of the NewSelectionOnDraw
            if ((_behaviour.Input.DoSelectL || _behaviour.Input.DoSelectR) &&
                !(_behaviour.Input.DoSelectLPrev || _behaviour.Input.DoSelectRPrev)) // Just started stroke 
            {
                if (InputManager.State.NewSelectionOnDraw) // Active selection not empty
                {
                    // Increment or add until we get an empty selection
                    while (_behaviour.State->SSizes[_behaviour.Input.ActiveSelectionId] > 0)
                    {
                        if (_behaviour.Input.ActiveSelectionId < _behaviour.Input.SCountUi - 1)
                            _behaviour.SetActiveSelectionIncrement(1);
                        else
                        {
                            AddSelection();
                            break;
                        }
                    }
                }

                if (InputManager.State.DiscardSelectionOnDraw)
                    _behaviour.Input.DoClearSelection |= 1U << _behaviour.Input.ActiveSelectionId;
            }

            // Copy UI selection count to the state (as it is shared with C++)
            _behaviour.State->SSize = _behaviour.Input.SCountUi;
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
        /// Repaint based on which mesh is active. Should be called when <see cref="MeshManager.OnActiveMeshChanged"/>
        /// </summary>
        /// <remarks>Also called when the object is created as this happens just after a mesh was set</remarks>
        private void RepaintActiveMesh()
        {
            var isActive = _behaviour.Mesh.IsActiveMesh();
            activeImage.sprite = isActive ? activeSprite : editSprite;
            background.color = isActive ? activeBackgroundColor : _defaultBackgroundColor;
        }

        /// <summary>
        /// Repaint the active selection. Should be called when <see cref="LibiglBehaviour.OnActiveSelectionChanged"/>.
        /// </summary>
        private void RepaintActiveSelection()
        {
            // Repaint the selection UI
            if (_activeSelectionId >= 0)
                _selections[_activeSelectionId].ToggleEditSprite(false);
            _activeSelectionId = _behaviour.Input.ActiveSelectionId;
            _selections[_activeSelectionId].ToggleEditSprite(true);

        }

        #endregion

        #region Helper Functions

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

        private void UpdateVertexCountText()
        {
            _vertexCount.text =
                $"<b>V</b>: {_behaviour.State->SSizesAll}/{_behaviour.State->VSize} <b>F</b>: {_behaviour.State->FSize}";
        }

        #endregion
    }
}