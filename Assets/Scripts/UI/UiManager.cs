using Libigl;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
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

        // Prefabs for generating UI, e.g. Details panel
        public GameObject listCanvasPrefab;
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

        public Transform panelSpawnPoint;
        
        [Tooltip("The Content of the Actions Canvas scroll list.")]
        public Transform actionsListParent;

        private UiCollapsible _toolGroup;
        private UiCollapsible _meshGroup;
        private UiCollapsible _debugGroup;

        [Tooltip("Debug material placed on objects when enabled.")]
        public Material uvGridMaterial;
        [Tooltip("Replaces the first material of the renderer with the UV grid when enabled.")]
        public MeshRenderer[] toggleUvGridRenderers;
        private bool _isShowingUvGrid;
        private Material[] _uvGridInitialMaterials;

        private LayerMask _uiLayerMask;

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
            _uiLayerMask = LayerMask.GetMask("UI");
        }

        /// <summary>
        /// Creates a new Details panel and initializes it
        /// </summary>
        /// <returns>The Vertical Scroll List parent to which items can be added as a child</returns>
        public UiDetails CreateDetailsPanel()
        {
            var go = Instantiate(listCanvasPrefab, panelSpawnPoint.position, panelSpawnPoint.rotation, transform);
            go.GetComponent<Canvas>().worldCamera = Camera.main;

            // Move the panel until it is not colliding
            var t = go.transform;
            var results = new Collider[1];
            while (Physics.OverlapSphereNonAlloc(t.position, 0.4f, results, _uiLayerMask, QueryTriggerInteraction.Ignore) > 0)
            {
                t.Translate(-Vector3.right * 0.8f);
            }

            return go.GetComponent<UiDetails>();
        }


        /// <summary>
        /// Generates the UI unrelated to a mesh or to manipulate the <i>active mesh</i> <see cref="MeshManager.ActiveMesh"/>
        /// </summary>
        private unsafe void InitializeStaticUi()
        {
            // Meshes
            _meshGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _meshGroup.title.text = "Load Mesh";
            _meshGroup.SetVisibility(true);

            foreach (var meshPrefab in MeshManager.get.meshPrefabs)
            {
                // Create button to load each mesh
                var iconAction = Instantiate(iconActionPrefab, actionsListParent).GetComponent<UiIconAction>();
                _meshGroup.AddItem(iconAction.gameObject);
                var textField = iconAction.actionBtn.GetComponentInChildren<TMP_Text>();
                textField.text = meshPrefab.name;

                // setup callbacks/events
                iconAction.actionBtn.onClick.AddListener(() => MeshManager.get.LoadMesh(meshPrefab));
                iconAction.iconBtn.onClick.AddListener(() => MeshManager.get.LoadMesh(meshPrefab, false));
                
                var validMesh = MeshManager.get.CheckPrefabValidity(meshPrefab);
                iconAction.actionBtn.interactable = validMesh;
                iconAction.iconBtn.interactable = validMesh;
            }
            
            
            // Tools
            _toolGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _toolGroup.title.text = "Tools & Actions";
            _toolGroup.SetVisibility(true);
            
            var selectionMode = Instantiate(selectionModePrefab, actionsListParent).GetComponent<UiSelectionMode>();
            _toolGroup.AddItem(selectionMode.gameObject);
            selectionMode.Initialize();
            
            var pivotMode = Instantiate(pivotModePrefab, actionsListParent).GetComponent<UiPivotMode>();
            _toolGroup.AddItem(pivotMode.gameObject);
            pivotMode.Initialize();

            CreateActionUi("Default Tool",
                () => { InputManager.get.SetActiveTool(ToolType.Default); }, _toolGroup);
            CreateActionUi("Select Tool",
                () => { InputManager.get.SetActiveTool(ToolType.Select); }, _toolGroup,
                new[] {"select"});

            CreateActionUi("Harmonic", () => { MeshManager.ActiveMesh.Behaviour.Input.DoHarmonic = true; }, _toolGroup,
                new[] {"smooth", "harmonic", "laplacian"});
            CreateActionUi("Arap", () => { MeshManager.ActiveMesh.Behaviour.Input.DoArap = true; }, _toolGroup,
                new[] {"rigid"});
            CreateActionUi("Translate", () => { MeshManager.ActiveMesh.Behaviour.Input.DoTransform = true; }, _toolGroup,
                new[] {"translate", "move"});
            CreateActionUi("Do Select", () => { MeshManager.ActiveMesh.Behaviour.Input.DoSelect = true; }, _toolGroup);

            
            // Debug
            _debugGroup = Instantiate(groupPrefab, actionsListParent).GetComponent<UiCollapsible>();
            _debugGroup.title.text = "Debug";
            _debugGroup.SetVisibility(true);
            
            var toggleUvGridAction = Instantiate(buttonPrefab, actionsListParent).GetComponent<Button>();
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
                    toggleUvGridRenderers[i].material =  _isShowingUvGrid ? uvGridMaterial : _uvGridInitialMaterials[i];
            });
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
