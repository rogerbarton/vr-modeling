using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace libigl.Behaviour
{
    public unsafe partial class LibiglBehaviour
    {
        private void UpdateInput()
        {
            if (InputManager.get.RightHand.isValid)
            {
                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.secondaryButton,
                    out var secondaryBtnValue) && secondaryBtnValue)
                {
                    _input.Translate = true;
                }


                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primaryButton, out var primaryBtnValue) &&
                    primaryBtnValue)
                {
                    _input.Select = true;
                    if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.devicePosition, 
                        out var rightHandPos))
                    {
                        _input.SelectPos = _libiglMesh.transform.InverseTransformPoint(
                                InputManager.get.XRRig.TransformPoint(rightHandPos));
                    }
                    else 
                        Debug.LogWarning("Could not get Right Hand Position");
                }

                if (InputManager.get.RightHand.TryGetFeatureValue(CommonUsages.primary2DAxis, out var primaryAxisValue))
                {
                    if (Mathf.Abs(primaryAxisValue.y) > 0.01f)
                    {
                        _input.SelectRadiusSqr = Mathf.Clamp(Mathf.Sqrt(_input.SelectRadiusSqr) + 0.5f * primaryAxisValue.y * Time.deltaTime, 
                            0.01f, 1f);
                        _input.SelectRadiusSqr *= _input.SelectRadiusSqr;
                    }
                }
            }
        }

        /// <summary>
        /// Consumes and resets flags raised. Should be called in PreExecute after copying to the State.
        /// </summary>
        private void ConsumeInput()
        {
            // Consume inputs here
            _input.Translate = false;
            _input.Select = false;
            _input.Harmonic = false;
        }

        /// <summary>
        /// Static method that generates the UI to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        public static void InitializeActionUi()
        {
            UiManager.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour._input.Translate = true; }, new []{"translate", "move"}, 1);
            UiManager.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour._input.Harmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
            
            // Tools TODO change tool with UI
            UiManager.get.CreateActionUi("Select", () => { MeshManager.ActiveMesh.Behaviour._input.Select = true; }, new [] {"select"});
        }

        
        public class UiDetails 
        {
            public TMP_Text MeshName;
            public TMP_Text VertexCount;
            public TMP_Text FaceCount;
            public TMP_Text SelectCount;

            public Button SetActiveBtn;
            public Button ResetTransformBtn;
            
            public Button ToggleWireframe;
            
            public Button AddSelectionBtn;

            private LibiglBehaviour _behaviour;
            private Transform _canvas;
            private Transform _listParent;

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
                SelectCount.text = $"Selected: {_behaviour._state->SSize}";
                
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
                var selectionHeader = Object.Instantiate(UiManager.get.headerPrefab, _listParent).GetComponent<TMP_Text>();
                selectionHeader.text = "Selection";
                
                AddSelectionBtn = Object.Instantiate(UiManager.get.buttonPrefab, _listParent).GetComponent<Button>();
                AddSelectionBtn.GetComponentInChildren<TMP_Text>().text = "Add Selection";
                AddSelectionBtn.onClick.AddListener(() => { /*TODO*/ });
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
                if (_behaviour._state->Input.Select) 
                    SelectCount.text = $"Selected: {_behaviour._state->SSize}";
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
        }
    }
}