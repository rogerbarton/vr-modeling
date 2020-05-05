using System;
using System.Collections.Generic;
using System.Linq;
using libigl;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

/// <summary>
/// Handles easy creation of operations to be done on a mesh and the user interaction (2D UI, speech, gestures)
/// that comes with it.
/// </summary>
public class MeshActions : MonoBehaviour
{
    [NonSerialized] public readonly List<MeshAction> InitializeActions = new List<MeshAction>();
    [NonSerialized] public readonly List<MeshAction> UpdateActions = new List<MeshAction>();
    
    public Transform uiListItemParent;
    private GameObject _uiListItemPrefab;
    
    public static MeshActions get;

    private void Awake()
    {
        if (get == null)
            get = this;
        else
        {
            Debug.LogWarning("Instance already exists.");
            return;
        }
        
        // Convention: Use the first child as the prefab
        if (!_uiListItemPrefab && uiListItemParent.childCount > 0)
            _uiListItemPrefab = uiListItemParent.GetChild(uiListItemParent.childCount -1).gameObject;
        
        RegisterAction(new MeshAction(MeshActionType.OnInitialize, "OnInitialize",
            data =>
            {
                var tmp = (uint) data.DirtyState;
                Native.InitializeMesh(data.GetNative(), ref tmp);
                data.DirtyState = (MeshData.DirtyFlag) tmp;
            }, 
            () => true));
        
        RegisterAction(new MeshAction(MeshActionType.OnUpdate,
            "Select",
            null,
            -1,
            new SelectAction()));
    }

    /// <summary>
    /// Will register an action to enable its entry points, i.e. UI generation, gesture recognition
    /// Actions are always executed in order in which they were registered.
    /// </summary>
    public void RegisterAction(MeshAction action)
    {
        if(action.Type == MeshActionType.OnInitialize)
        {
            InitializeActions.Add(action);
        }
        else if (action.Type == MeshActionType.OnUpdate)
        {
            get.SetupActionUi(action);
            UpdateActions.Add(action);
        }
    }

    /// <summary>
    /// Generates UI, gesture and speed entry points based on an action
    /// </summary>
    private void SetupActionUi(MeshAction action)
    {
        // Parenting, layout, ui
        var go = Instantiate(_uiListItemPrefab, uiListItemParent);
        go.SetActive(true);
        var textField = go.GetComponentInChildren<TMP_Text>();
        textField.text = action.Name;

        // setup callbacks/events
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(() => action.Schedule());

        // Setup speech keywords
        action.InitializeSpeech();

        // TODO: Setup gesture recognition
    }

    private void Update()
    {
        foreach (var action in UpdateActions.Where(action => action.ExecuteCondition()))
            action.Schedule();
    }

    private void OnDestroy()
    {
        PhraseRecognitionSystem.Shutdown();
        foreach (var action in UpdateActions)
            action.Dispose();
        
        // Clear static variables (only required when Domain Reload is disables in the Editor)
        Speech.Reset();
        InitializeActions.Clear();
        UpdateActions.Clear();
    }
}
