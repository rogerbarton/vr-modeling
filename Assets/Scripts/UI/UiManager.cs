using libigl.Behaviour;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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

        public Transform panelSpawnPoint;
        
        [Tooltip("The Content of the Actions Canvas scroll list. Convention: the last child serves as the prefab for a new item.")]
        public Transform actionsListParent;
        private GameObject _actionsListPrefab;
    

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("UIActions instance already exists.");
                enabled = false;
                return;
            }
            get = this;
        
            // Convention: Use the last child as the prefab
            if (!_actionsListPrefab && actionsListParent.childCount > 0)
                _actionsListPrefab = actionsListParent.GetChild(actionsListParent.childCount -1).gameObject;

            LibiglBehaviour.InitializeActionUi();
        }

        /// <summary>
        /// Generates UI, gesture and speed entry points based on an action
        /// </summary>
        /// <param name="onClick">Code to execute when an entry point is triggered</param>
        public void CreateActionUi(string uiText, UnityAction onClick, string[] speechKeywords = null, int gestureId = -1)
        {
            // Parenting, layout, ui
            var go = Instantiate(_actionsListPrefab, actionsListParent);
            go.SetActive(true);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = uiText;

            // setup callbacks/events
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(onClick);

            // Setup speech keywords
            Speech.CreateKeywordRecognizer(speechKeywords, onClick);

            // TODO: Setup gesture recognition
            if (gestureId >= 0)
            {
            }
        }

        /// <summary>
        /// Creates a new Details panel and initializes it
        /// </summary>
        /// <returns>The Vertical Scroll List parent to which items can be added as a child</returns>
        public Transform CreateDetailsPanel()
        {
            var go = Instantiate(listCanvasPrefab, panelSpawnPoint.position, panelSpawnPoint.rotation, transform);
            go.GetComponent<Canvas>().worldCamera = Camera.main;
            
            return go.GetComponentInChildren<VerticalLayoutGroup>().transform;
        }
        
        private void OnDestroy()
        {
            Speech.Dispose();
        }

    }
}
