using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        /// <summary>
        /// Static method that generates the UI to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        public static void InitializeActionUi()
        {
            UiManager.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour._input.DoTransform = true; }, new []{"translate", "move"}, 1);
            UiManager.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour._input.DoHarmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
            
            // Tools TODO change tool with UI
            UiManager.get.CreateActionUi("Select", () => { MeshManager.ActiveMesh.Behaviour._input.DoSelect = true; }, new [] {"select"});
        }

        
        public class UiDetails 
        {
            private LibiglBehaviour _behaviour;
            private Transform _canvas;
            private Transform _listParent;
         
            // UI Components
            public TMP_Text MeshName;
            public TMP_Text VertexCount;
            public TMP_Text FaceCount;
            public TMP_Text SelectCount;

            public Button SetActiveBtn;
            public Button ResetTransformBtn;
            
            public Button ToggleWireframe;
            
            public UiCollapsible SelectionGroup;
            public Button AddSelectionBtn;
            public List<UiSelection> Selections = new List<UiSelection>(); 

            public void Initialize(LibiglBehaviour behaviour)
            {
                _behaviour = behaviour;
                MeshManager.ActiveMeshChanged += ActiveMeshChanged;

                _canvas = UiManager.get.CreateDetailsPanel();
                _listParent = _canvas.GetComponentInChildren<VerticalLayoutGroup>().transform;

                MeshName = Object.Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
                MeshName.text = _behaviour._libiglMesh.name;
                
                VertexCount = Object.Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
                VertexCount.text = $"Vertices: {_behaviour._state->VSize}";
                
                FaceCount = Object.Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
                FaceCount.text = $"Faces: {_behaviour._state->FSize}";
                
                SelectCount = Object.Instantiate(UiManager.get.textPrefab, _listParent).GetComponent<TMP_Text>();
                SelectCount.text = $"Selected: {_behaviour._state->SSizeAll}";
                
                // Set mesh as the active one if not active already
                SetActiveBtn = Object.Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
                SetActiveBtn.GetComponentInChildren<TMP_Text>().text = "Set As Active";
                SetActiveBtn.onClick.AddListener(() => { MeshManager.SetActiveMesh(_behaviour._libiglMesh); });
                SetActiveBtn.gameObject.SetActive(!_behaviour._libiglMesh.IsActiveMesh());
                
                // Reset Transform to spawnPoint button 
                ResetTransformBtn = Object.Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
                ResetTransformBtn.GetComponentInChildren<TMP_Text>().text = "Reset Transform";
                ResetTransformBtn.onClick.AddListener(() => { behaviour._libiglMesh.ResetTransformToSpawn(); });
                
                // Shaders
                // var ShaderHeader = Object.Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
                // ShaderHeader.text = "Shader";
                
                ToggleWireframe = Object.Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
                ToggleWireframe.GetComponentInChildren<TMP_Text>().text = "Toggle Wireframe";
                ToggleWireframe.onClick.AddListener(() =>
                {
                    var materials = behaviour._libiglMesh.MeshRenderer.materials;
                    if (materials.Length == 1)
                        materials = materials.Append(MeshManager.get.wireframeMaterial).ToArray();
                    else
                        materials = new[] {materials.First()};
                    behaviour._libiglMesh.MeshRenderer.materials = materials;
                });
                
                // Selection
                SelectionGroup = Object.Instantiate(UiManager.get.groupPrefab, _listParent).GetComponent<UiCollapsible>();
                SelectionGroup.title.text = "Selection";
                
                AddSelectionBtn = Object.Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
                AddSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
                AddSelectionBtn.onClick.AddListener(AddSelection);
                AddSelection();
            }

            private void AddSelection()
            {
                // Get the id and set it as the active one
                var selectionId = _behaviour._input.SCount;
                _behaviour._input.SCount++;
                _behaviour._input.ActiveSelectionId = selectionId;
                
                // Add UI for the selection
                var uiSelection = Object.Instantiate(UiManager.get.selectionPrefab, _listParent).GetComponent<UiSelection>();
                uiSelection.text.text = $"{selectionId}: 0 vertices";
                SelectionGroup.GetComponent<UiCollapsible>().AddItem(uiSelection.gameObject);
                uiSelection.editBtn.image.color = Util.Colors.GetColorById(selectionId);

                // Behaviour when clicking buttons
                uiSelection.editBtn.onClick.AddListener(() => { _behaviour._input.ActiveSelectionId = selectionId; });
                uiSelection.clearBtn.onClick.AddListener(() => { _behaviour._input.DoClearSelection |= 1 << selectionId; });
                
                Selections.Add(uiSelection);
            }

            public void Deconstruct()
            {
                MeshManager.ActiveMeshChanged -= ActiveMeshChanged;
                if(_canvas)
                    Object.Destroy(_canvas.gameObject);
            }

            /// <summary>
            /// Update the UI Details panel after executing
            /// </summary>
            public void UpdatePostExecute()
            {
                // Update Selection UI
                if (_behaviour._input.DoSelect)
                {
                    SelectCount.text = $"Selected: {_behaviour._state->SSizeAll}";
                    for (var i = 0; i < Selections.Count; i++)
                        Selections[i].GetComponentInChildren<TMP_Text>().text =
                            $"{i}: {_behaviour._state->SSize[i]} vertices";
                }
            }

            /// <summary>
            /// Called in PreExecute just before the input is consumed
            /// </summary>
            public void UpdatePreExecute()
            {
                
            }

            /// <summary>
            /// Called when the active mesh changes
            /// </summary>
            private void ActiveMeshChanged()
            {
                SetActiveBtn.gameObject.SetActive(!_behaviour._libiglMesh.IsActiveMesh());
            }

            public void ActiveSelectionChanged()
            {
                // TODO: Update UI that shows active selection
            }
        }
    }
}