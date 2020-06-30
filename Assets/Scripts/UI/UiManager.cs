using System;
using Libigl;
using TMPro;
using UI.Components;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Util;
using XrInput;

namespace UI
{
    /// <summary>
    /// Handles easy creation of operations to be done on a mesh and the user interaction (2D UI, speech, gestures)
    /// that comes with it.
    /// </summary>
    public class UiManager : MonoBehaviour
    {
        public static UiManager get;

        #region Prefabs for generating UI
        
        [Header("UI Component Prefabs")]
        public GameObject meshDetailsCanvasPrefab;
        public GameObject headerPrefab;
        public GameObject groupPrefab;
        public GameObject textPrefab;
        public GameObject buttonPrefab;
        public GameObject togglePrefab;
        public GameObject toggleActionPrefab;
        public GameObject iconActionPrefab;
        public GameObject selectionPrefab;
        public GameObject selectionModePrefab;
        public GameObject pivotModePrefab;

        #endregion

        [Header("Other")]
        public Transform panelSpawnPoint;

        [Tooltip("The Content of the Actions Canvas scroll list.")]
        public Transform genericUiListParent;

        [Tooltip("Debug material placed on objects when enabled.")]
        public Material uvGridMaterial;

        [Tooltip("Replaces the first material of the renderer with the UV grid when enabled.")]
        public MeshRenderer[] toggleUvGridRenderers;

        private bool _isShowingUvGrid;
        private Material[] _uvGridInitialMaterials;

        [NonSerialized] public LayerMask UiLayerMask;
        [NonSerialized] public const int UiLayer = 5;

        // -- UI instances
        private UiCollapsible _toolGroup;
        private UiCollapsible _meshGroup;
        private UiCollapsible _debugGroup;
        [NonSerialized] public UiPivotMode PivotMode;

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
            UiLayerMask = LayerMask.GetMask("UI");
            InitializeStaticUi();
        }

        /// <summary>
        /// Creates a new Details panel and initializes it
        /// </summary>
        /// <returns>The Vertical Scroll List parent to which items can be added as a child</returns>
        public UiMeshDetails CreateDetailsPanel()
        {
            var go = Instantiate(meshDetailsCanvasPrefab, panelSpawnPoint.position, panelSpawnPoint.rotation, transform);
            go.GetComponent<Canvas>().worldCamera = Camera.main;

            // Move the panel until it is not colliding
            // Remove it from the UI layer so we don't collide with it below
            var layer = go.layer;
            go.layer = 0;

            var t = go.transform;
            var results = new Collider[1];
            while (Physics.OverlapSphereNonAlloc(t.position, 0.4f, results, UiLayerMask,
                QueryTriggerInteraction.Ignore) > 0)
            {
                t.Translate(-Vector3.right * 0.8f);
            }

            go.layer = layer;

            return go.GetComponent<UiMeshDetails>();
        }


        /// <summary>
        /// Generates the UI unrelated to a mesh or to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        private void InitializeStaticUi()
        {
            // -- Tools
            _toolGroup = Instantiate(groupPrefab, genericUiListParent).GetComponent<UiCollapsible>();
            _toolGroup.title.text = "Tools & Actions";
            _toolGroup.SetVisibility(true);

            var selectionMode = Instantiate(selectionModePrefab, genericUiListParent).GetComponent<UiSelectionMode>();
            _toolGroup.AddItem(selectionMode.gameObject);
            selectionMode.Initialize();

            PivotMode = Instantiate(pivotModePrefab, genericUiListParent).GetComponent<UiPivotMode>();
            _toolGroup.AddItem(PivotMode.gameObject);
            PivotMode.Initialize();


            // -- Meshes
            _meshGroup = Instantiate(groupPrefab, genericUiListParent).GetComponent<UiCollapsible>();
            _meshGroup.title.text = "Load Mesh";
            _meshGroup.SetVisibility(true);

            foreach (var meshPrefab in MeshManager.get.meshPrefabs)
            {
                // Create button to load each mesh
                var iconAction = Instantiate(iconActionPrefab, genericUiListParent).GetComponent<UiIconAction>();
                _meshGroup.AddItem(iconAction.gameObject);
                var textField = iconAction.actionBtn.GetComponentInChildren<TMP_Text>();
                textField.text = meshPrefab.name;

                // Setup callbacks/events
                iconAction.actionBtn.onClick.AddListener(() => MeshManager.get.LoadMesh(meshPrefab));
                iconAction.iconBtn.onClick.AddListener(() => MeshManager.get.LoadMesh(meshPrefab, false));

                var validMesh = MeshManager.CheckPrefabValidity(meshPrefab);
                iconAction.actionBtn.interactable = validMesh;
                iconAction.iconBtn.interactable = validMesh;
            }


            // -- Debug
            _debugGroup = Instantiate(groupPrefab, genericUiListParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Debug";
            _debugGroup.SetVisibility(true);

            var toggleBounds = Instantiate(UiManager.get.buttonPrefab, genericUiListParent).GetComponent<Button>();
            _debugGroup.AddItem(toggleBounds.gameObject);
            toggleBounds.GetComponentInChildren<TMP_Text>().text = "Toggle Bounds";
            toggleBounds.onClick.AddListener(() =>
            {
                InputManager.State.BoundsVisible = !InputManager.State.BoundsVisible;
                foreach (var mesh in MeshManager.get.AllMeshes)
                    mesh.RepaintBounds();
            });

            var toggleUvGridAction = Instantiate(buttonPrefab, genericUiListParent).GetComponent<Button>();
            _debugGroup.AddItem(toggleUvGridAction.gameObject);
            toggleUvGridAction.GetComponentInChildren<TMP_Text>().text = "Toggle UV Grid";

            // Get references to normal materials
            _uvGridInitialMaterials = new Material[toggleUvGridRenderers.Length];
            for (var i = 0; i < toggleUvGridRenderers.Length; i++)
                _uvGridInitialMaterials[i] = toggleUvGridRenderers[i].material;

            toggleUvGridAction.onClick.AddListener(() =>
            {
                // Toggle uv debug material
                _isShowingUvGrid = !_isShowingUvGrid;
                for (var i = 0; i < toggleUvGridRenderers.Length; i++)
                    toggleUvGridRenderers[i].material = _isShowingUvGrid ? uvGridMaterial : _uvGridInitialMaterials[i];
            });
        }

        /// <summary>
        /// Generates UI, gesture and speed entry points based on an action
        /// </summary>
        /// <param name="onClick">Code to execute when an entry point is triggered</param>
        /// <param name="collapsible">The group to add this item under</param>
        private void CreateActionSpeechUi(string uiText, UnityAction onClick, UiCollapsible collapsible = null,
            string[] speechKeywords = null)
        {
            // Parenting, layout, ui
            var go = Instantiate(buttonPrefab, genericUiListParent);
            if (collapsible != null)
                collapsible.AddItem(go);
            else
                go.SetActive(true);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = uiText;

            // setup callbacks/events
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            // Setup speech keywords
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Speech.CreateKeywordRecognizer(speechKeywords, onClick);
#endif
        }

        private void OnDestroy()
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            Speech.Dispose();
#endif
        }

    }
}