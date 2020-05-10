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
            UiManager.get.CreateActionUi("Test", () => Debug.Log("Test"), new []{"test"}, 0);
            UiManager.get.CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour._input.Translate = true; }, new []{"translate", "move"}, 1);
            UiManager.get.CreateActionUi("Select", () => { MeshManager.ActiveMesh.Behaviour._input.Select = true; }, new [] {"select"});
            UiManager.get.CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour._input.Harmonic = true; }, new [] {"smooth", "harmonic", "laplacian"}, 2);
        }

        
        public class UiDetails : Object 
        {
            public TMP_Text MeshName;
            public TMP_Text VertexCount;
            public TMP_Text FaceCount;
            public TMP_Text SelectCount;

            public Button SetActiveBtn;

            private LibiglBehaviour _behaviour;

            public void Initialize(LibiglBehaviour behaviour)
            {
                _behaviour = behaviour;
                MeshManager.ActiveMeshChanged += ActiveMeshChanged;

                var ListParent = UiManager.get.CreateDetailsPanel();

                MeshName = Instantiate(UiManager.get.headerPrefab, ListParent).GetComponent<TMP_Text>();
                MeshName.text = _behaviour._libiglMesh.name;
                
                VertexCount = Instantiate(UiManager.get.textPrefab, ListParent).GetComponent<TMP_Text>();
                VertexCount.text = $"Vertices: {_behaviour._state->VSize}";
                
                FaceCount = Instantiate(UiManager.get.textPrefab, ListParent).GetComponent<TMP_Text>();
                FaceCount.text = $"Faces: {_behaviour._state->FSize}";
                
                SelectCount = Instantiate(UiManager.get.textPrefab, ListParent).GetComponent<TMP_Text>();
                SelectCount.text = $"Selected: {_behaviour._state->SSize}";
                
                SetActiveBtn = Instantiate(UiManager.get.buttonPrefab, ListParent).GetComponent<Button>();
                SetActiveBtn.GetComponentInChildren<TMP_Text>().text = "Set As Active";
                SetActiveBtn.onClick.AddListener(() => { MeshManager.SetActiveMesh(_behaviour._libiglMesh); });
                SetActiveBtn.gameObject.SetActive(_behaviour._libiglMesh.IsActiveMesh());
            }

            public void Deconstruct()
            {
                MeshManager.ActiveMeshChanged -= ActiveMeshChanged;
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
                SetActiveBtn.gameObject.SetActive(_behaviour._libiglMesh.IsActiveMesh());
            }
        }
    }
}