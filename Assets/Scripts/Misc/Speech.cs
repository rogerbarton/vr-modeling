#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Windows.Speech;

public static class Speech
{
    private static readonly List<string> AllKeywords = new List<string>();
    private static readonly List<KeywordRecognizer> KeywordRecognizers = new List<KeywordRecognizer>();
    private static bool _enabled;
    
    /// <summary>
    /// Creates a new KeywordRecognizer that calls <paramref name="onRecognized"/> when any keyword in <paramref name="speechKeywords"/> is heard and recognized.
    /// Keywords that are already used will be ignored.
    /// </summary>
    /// <param name="speechKeywords">Keywords to listen for</param>
    /// <param name="onRecognized">UnityAction to execute when any keyword is recognized</param>
    public static void CreateKeywordRecognizer(string[] speechKeywords, UnityAction onRecognized)
    {
        if (speechKeywords == null) return;

        CheckSpeechKeywords(ref speechKeywords);
        if (speechKeywords.Length <= 0) return;
        
        var recognizer = new KeywordRecognizer(speechKeywords);
        recognizer.OnPhraseRecognized += _ => { onRecognized(); };
        if(_enabled)
            recognizer.Start();
        KeywordRecognizers.Add(recognizer);
    }
    
    /// <summary>
    /// Checks if the keywords are valid and that there aren't any conflicts. Conflicts are removed with a warning.
    /// </summary>
    private static void CheckSpeechKeywords(ref string[] speechKeywords)
    {
        if (speechKeywords.Intersect(AllKeywords).Any())
            Debug.LogWarning("Speech keyword/s duplicated: " + string.Join(",", speechKeywords.Intersect(AllKeywords)));

        speechKeywords = speechKeywords.Except(AllKeywords).ToArray();
        AllKeywords.AddRange(speechKeywords);
    }
    
    /// <summary>
    /// Turn speech recognition on and off dynamically
    /// </summary>
    public static void SetEnabled(bool value)
    {
        if (!_enabled && value)
        {
            foreach (var recognizer in KeywordRecognizers.Where(recognizer => !recognizer.IsRunning))
                recognizer.Start();
        }
        
        if (_enabled && !value)
        {
            foreach (var recognizer in KeywordRecognizers.Where(recognizer => recognizer.IsRunning))
                recognizer.Stop();
        }

        _enabled = value;
    }
    
    public static void Dispose()
    {
        AllKeywords.Clear();
        foreach (var recognizer in KeywordRecognizers)
            recognizer.Dispose();
    }
}
#endif