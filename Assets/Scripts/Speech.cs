using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Speech
{
    private static List<string> AllKeywords = new List<string>();

    /// <summary>
    /// Checks if the keywords are valid and that there aren't any conflicts
    /// </summary>
    /// <param name="speechKeywords">Speech keywords</param>
    public static void CheckSpeechKeywords(ref string[] speechKeywords)
    {
        if (speechKeywords.Intersect(AllKeywords).Any())
            Debug.LogWarning("Speech keyword/s duplicated: " + string.Join(",", speechKeywords.Intersect(AllKeywords)));

        speechKeywords = speechKeywords.Except(AllKeywords).ToArray();
        AllKeywords.AddRange(speechKeywords);
    }
    
    public static void Reset()
    {
        AllKeywords.Clear();
    }
}
