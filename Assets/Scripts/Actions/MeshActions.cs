using System.Collections.Generic;
using System.Linq;
using libigl;
using libigl.Samples;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

/// <summary>
/// Handles easy creation of operations to be done on a mesh and the user interaction (2D UI, speech, gestures)
/// that comes with it.
/// </summary>
public class MeshActions : MonoBehaviour
{
    public List<MeshAction> actions = new List<MeshAction>();
    public Transform uiListItemParent;
    private GameObject _uiListItemPrefab;

    private KeywordRecognizer _keywordRecognizer;
    private List<string> _allKeywords = new List<string>();

    private static MeshActions get;

    private void Start()
    {
        if (get == null)
            get = this;
        else
        {
            Debug.LogWarning("Instance already exists.");
            return;
        }

        // Add all actions
        actions.Add(
            new MeshAction(
                "Test",
                new[] {"Test"},
                0,
                () => Input.GetKeyDown(KeyCode.Q),
                _ => { Debug.Log("Execute Test"); }));

        actions.Add(
            new MeshAction(
                "Translate",
                new[] {"move", "translate"},
                1,
                new TranslateAction()));

        actions.Add(
            new MeshAction(
                "Smooth",
                new[] {"smooth", "harmonic", "laplacian"},
                2,
                () => Input.GetKeyDown(KeyCode.E),
                data =>
                {
                    unsafe
                    {
                        Native.Harmonic((float*) data.V.GetUnsafePtr(), data.VSize, (int*) data.F.GetUnsafePtr(),
                            data.FSize);
                    }

                    data.DirtyState |= MeshData.DirtyFlag.VDirty;
                    data.DirtyState |= MeshData.DirtyFlag.ComputeNormals;
                }));

        actions.Add(
            new MeshAction(
                "Select",
                null,
                -1,
                new SelectAction()));

        actions.Add(new MeshAction("CustomUpdate",
        data =>
        {
            // Example where everything is done in C++, we can also pass additional arguments about the input state if we wanted
            // This will be called "every frame" unless there is already some computation running
            var tmp = (uint) data.DirtyState;
            Native.CustomUpdateSample(data.GetNative(), ref tmp);
            data.DirtyState = (MeshData.DirtyFlag) tmp;
        }, 
        () => false)); // Change this to true to test sample

        InitializeUI();
    }

    private void InitializeUI()
    {
        // Convention: Use the first child as the prefab
        if (!_uiListItemPrefab && uiListItemParent.childCount > 0)
            _uiListItemPrefab = uiListItemParent.GetChild(0).gameObject;
        
        // Create listitem foreach action
        foreach (var a in actions)
            SetupAction(a);

        // Create one speech keywords recognizer for all actions
        _keywordRecognizer = new KeywordRecognizer(_allKeywords.ToArray());
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();
    }

    /// <summary>
    /// Will register an action to enable its entry points, i.e. UI generation, gesture recognition
    /// </summary>
    public static void RegisterAction(MeshAction action)
    {
        get.SetupAction(action);
        get.actions.Add(action);
    }

    private void SetupAction(MeshAction action)
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
        if (action.SpeechKeywords != null)
            _allKeywords.AddRange(action.SpeechKeywords);

        // TODO: Setup gesture recognition
    }

    /// <summary>
    /// Invoked by the KeywordRecognizer, this will schedule the according action.
    /// </summary>
    /// <param name="args"></param>
    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        foreach (var action in actions)
        {
            if (action.SpeechKeywords.Contains(args.text))
            {
                action.Schedule();
                break;
            }
        }
    }

    private void Update()
    {
        foreach (var action in actions)
        {
            if (action.ExecuteCondition())
                action.Schedule();
        }
    }

    private void OnDestroy()
    {
        _keywordRecognizer?.Dispose();
    }
}
