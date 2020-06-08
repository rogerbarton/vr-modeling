using System;
using libigl.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles easy creation of operations to be done on a mesh and the user interaction (2D UI, speech, gestures)
    /// that comes with it.
    /// </summary>
    public class UiManager : MonoBehaviour
    {
        public static UiManager get;

        // Prefabs for generating UI, e.g. Details panel
        public GameObject listCanvasPrefab;
        public GameObject headerPrefab;
        public GameObject textPrefab;
        public GameObject buttonPrefab;
        public GameObject groupPrefab;
        public GameObject selectionPrefab;
        public GameObject toggleActionPrefab;

        public Transform panelSpawnPoint;
        
        [Tooltip("The Content of the Actions Canvas scroll list. Convention: the last child serves as the prefab for a new item.")]
        public Transform actionsListParent;

        private UiCollapsible _toolGroup;
        private UiCollapsible _meshGroup;
        private UiCollapsible _debugGroup;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("UIActions instance already exists.");
                enabled = false;
                return;
            }
            get = this;
        }

        private void Start()
        {
            InitializeStaticUi();
        }

        /// <summary>
        /// Creates a new Details panel and initializes it
        /// </summary>
        /// <returns>The Vertical Scroll List parent to which items can be added as a child</returns>
        public UiDetails CreateDetailsPanel()
        {
            var go = Instantiate(listCanvasPrefab, panelSpawnPoint.position, panelSpawnPoint.rotation, transform);
            go.GetComponent<Canvas>().worldCamera = Camera.main;
            
            return go.GetComponent<UiDetails>();
        }


        /// <summary>
        /// Generates the UI unrelated to a mesh or to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        private unsafe void InitializeStaticUi()
        {
            // Tools
            _toolGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _toolGroup.title.text = "Tools & Actions";
            _toolGroup.SetVisibility(true);

            CreateActionUi("Default Tool",
                () => { MeshManager.ActiveMesh.Behaviour.Input.ActiveTool = ToolType.Default; }, _toolGroup);
            CreateActionUi("Select Tool",
                () => { MeshManager.ActiveMesh.Behaviour.Input.ActiveTool = ToolType.Select; }, _toolGroup,
                new[] {"select"});

            CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour.Input.DoHarmonicOnce = true; }, _toolGroup,
                new[] {"smooth", "harmonic", "laplacian"});
            CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour.Input.DoTransform = true; }, _toolGroup,
                new[] {"translate", "move"});
            CreateActionUi("Do Select", () => { MeshManager.ActiveMesh.Behaviour.Input.DoSelect = true; }, _toolGroup);

            // Meshes
            _meshGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _meshGroup.title.text = "Load Mesh";
            _meshGroup.SetVisibility(true);

            foreach (var meshPrefab in MeshManager.get.meshPrefabs)
            {
                // Create button to load each mesh
                var go = Instantiate(buttonPrefab, actionsListParent);
                _meshGroup.AddItem(go);
                var textField = go.GetComponentInChildren<TMP_Text>();
                textField.text = meshPrefab.name;

                // setup callbacks/events
                var button = go.GetComponent<Button>();
                button.onClick.AddListener(() => MeshManager.get.LoadMesh(meshPrefab));
                button.interactable = MeshManager.get.CheckPrefabValidity(meshPrefab);
            }

            // Debug
            _debugGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Debug";
            _debugGroup.SetVisibility(true);
        }

        /// <summary>
        /// Generates UI, gesture and speed entry points based on an action
        /// </summary>
        /// <param name="onClick">Code to execute when an entry point is triggered</param>
        /// <param name="collapsible">The group to add this item under</param>
        private void CreateActionUi(string uiText, UnityAction onClick, UiCollapsible collapsible = null, string[] speechKeywords = null)
        {
            // Parenting, layout, ui
            var go = Instantiate(buttonPrefab, actionsListParent);
            if(collapsible != null)
                collapsible.AddItem(go);
            else
                go.SetActive(true);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = uiText;

            // setup callbacks/events
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            // Setup speech keywords
            Speech.CreateKeywordRecognizer(speechKeywords, onClick);
        }

        private void OnDestroy()
        {
            Speech.Dispose();
        }

    }
}
