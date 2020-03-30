using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class SpeechTest : MonoBehaviour
{
    private KeywordRecognizer _keywordRecognizer;
    private Dictionary<string, Action> _actions = new Dictionary<string, Action>();
    
    public void Hello()
    {
        Debug.Log("Hello");
    }

    public void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Debug.Log($"OnPhraseRecognized {args.text}, confidence {args.confidence}");
        _actions[args.text].Invoke();
    }

    void Start()
    {
        _actions.Add("hello", Hello);
        _keywordRecognizer = new KeywordRecognizer(_actions.Keys.ToArray());
        _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        _keywordRecognizer.Start();
    }

    private void OnDestroy()
    {
        _keywordRecognizer.Stop();
        _keywordRecognizer.Dispose();
    }
}
