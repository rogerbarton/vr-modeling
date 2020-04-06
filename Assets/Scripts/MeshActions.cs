using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

[Serializable]
public class MeshAction
{
    public string name;
    public Action action;
    public string[] speechKeywords;
    public int gestureId;

    public MeshAction(string name, Action action, string[] speechKeywords, int gestureId)
    {
        this.name = name;
        this.action = action;
        this.speechKeywords = speechKeywords;
        this.gestureId = gestureId;
    }
}

/// <summary>
/// Handles easy creation of operations to be done on a mesh and the user interaction (2D UI, speech, gestures)
/// that comes with it.
/// </summary>
public class MeshActions : MonoBehaviour
{
    public List<MeshAction> actions;
    public GameObject uiListItemPrefab;
    public Transform uiListItemParent;
    
    private KeywordRecognizer keywordRecognizer;
    
    void Start()
    {
        // Add all actions
        actions.Add(
            new MeshAction(
                "Smooth",
                () =>
                {
                    Debug.Log("Execute Harmonic Smoothing");
                },
                new []{"smooth, harmonic, laplacian",},
                0));
        
        // Setup UI
        if (!uiListItemParent)
            uiListItemParent = transform;
        
        // Create listitem foreach action
        var allKeywords = new string[0];
        for (int i = 0; i < actions.Count; i++)
        {
            // Parenting, layout, ui
            var go = Instantiate(uiListItemPrefab, uiListItemParent);
            var textField = go.GetComponentInChildren<TMP_Text>();
            textField.text = actions[i].name;
            
            // setup callbacks/events
            var button = go.GetComponent<Button>();
            if(actions[i].action != null)
                button.onClick.AddListener(new UnityAction(actions[i].action));
            
            // Setup speech keywords
            allKeywords = allKeywords.Concat(actions[i].speechKeywords).ToArray();
            
            // TODO: Setup gesture recognition
        }
        
        // Create one speech keywords recognizer for all actions
        keywordRecognizer = new KeywordRecognizer(allKeywords);
        keywordRecognizer.Start();
    }

    private void OnDestroy()
    {
        keywordRecognizer.Dispose();
    }
}
