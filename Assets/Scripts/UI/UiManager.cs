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
        public Transform uiListItemParent;
        private GameObject _uiListItemPrefab;
    
        public static UiManager get;

        private void Awake()
        {
            if (get)
            {
                Debug.LogWarning("UIActions instance already exists.");
                enabled = false;
                return;
            }
            get = this;
        
            // Convention: Use the first child as the prefab
            if (!_uiListItemPrefab && uiListItemParent.childCount > 0)
                _uiListItemPrefab = uiListItemParent.GetChild(uiListItemParent.childCount -1).gameObject;

            LibiglBehaviour.InitializeActionUi();
        }

        /// <summary>
        /// Generates UI, gesture and speed entry points based on an action
        /// </summary>
        /// <param name="onClick">Code to execute when an entry point is triggered</param>
        public void CreateActionUi(string uiText, UnityAction onClick, string[] speechKeywords = null, int gestureId = -1)
        {
            // Parenting, layout, ui
            var go = Instantiate(_uiListItemPrefab, uiListItemParent);
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

        private void OnDestroy()
        {
            Speech.Dispose();
        }
    }
}
