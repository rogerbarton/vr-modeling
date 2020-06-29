#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Windows.Speech;

namespace Testing
{
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

        void OnEnable()
        {
            _actions.Add("hello", Hello);
            _keywordRecognizer = new KeywordRecognizer(_actions.Keys.ToArray());
            _keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
            _keywordRecognizer.Start();
        }

        private void OnDisable()
        {
            _keywordRecognizer.Stop();
            _keywordRecognizer.Dispose();
        }
    }
}
#endif