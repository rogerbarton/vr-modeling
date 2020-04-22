using System;
using System.Collections.Generic;
using System.Linq;
using libigl;
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
    public GameObject uiListItemPrefab;
    public Transform uiListItemParent;
    
    private KeywordRecognizer _keywordRecognizer;

    private void Start()
    {
        // Add all actions
        actions.Add(
            new MeshAction(
                "Test",
                new[] {"Test"},
                0,
                () => Input.GetKeyDown(KeyCode.Q),
                _ => { Debug.Log("Execute Test"); }, (_,__) => { Debug.Log("Apply Test"); }));
        
        actions.Add(
            new MeshAction(
                "Translate",
                new []{"move", "translate"},
                1,
                () => Input.GetKeyDown(KeyCode.W),
                new TranslateAction()));
        
        actions.Add(
            new MeshAction(
                "Smooth",
                new []{"smooth", "harmonic", "laplacian"},
                2,
                () => Input.GetKeyDown(KeyCode.E),
                data =>
                {
                    unsafe
                    {
                        Native.Harmonic(data.V.GetUnsafePtr(), data.VSize, data.F.GetUnsafePtr(), data.FSize);
                    }
                },
                (mesh, data) =>
                {
                    mesh.SetVertices(data.V);
                    mesh.RecalculateNormals();
                }));

        InitializeUI();
    }

    private void InitializeUI()
    {
        // Setup UI
        if (!uiListItemParent)
            uiListItemParent = transform;
        
        // Create listitem foreach action
        IEnumerable<string> allKeywords = new List<string>();
        for (int i = 0; i < actions.Count; i++)
        {
            // Parenting, layout, ui
            var go = Instantiate(uiListItemPrefab, uiListItemParent);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = actions[i].Name;
            
            // setup callbacks/events
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(actions[i].Schedule);
            
            // Setup speech keywords
            allKeywords = allKeywords.Concat(actions[i].SpeechKeywords);
            
            // TODO: Setup gesture recognition
        }
        
        // Create one speech keywords recognizer for all actions
        _keywordRecognizer = new KeywordRecognizer(allKeywords.ToArray());
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();
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
            if(action.ExecuteTest())
                action.Schedule();
        }
    }

    private void OnDestroy()
    {
        _keywordRecognizer?.Dispose();
    }
}
