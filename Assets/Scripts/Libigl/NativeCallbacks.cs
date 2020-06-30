using AOT;
using UnityEngine;

namespace Libigl
{
    /// <summary>
    /// Contains all callbacks from the native context
    /// C++ to C#
    /// </summary>
    /// <remarks>
    /// Callbacks should be annotated with the <see cref="MonoPInvokeCallbackAttribute"/> so that IL2CPP builds
    /// will compile properly
    /// </remarks>
    public static class NativeCallbacks
    {
        // Based on https://answers.unity.com/questions/30620/how-to-debug-c-dll-code.html
        // The 'function pointer type' passed to C++
        public delegate void StringCallback(string message);

        // The function that we point to in C++
        [MonoPInvokeCallback(typeof(StringCallback))]
        public static void DebugLog(string message)
        {
            Debug.Log("[c++] " + message);
        }

        [MonoPInvokeCallback(typeof(StringCallback))]
        public static void DebugLogWarning(string message)
        {
            Debug.LogWarning("[c++] " + message);
        }

        [MonoPInvokeCallback(typeof(StringCallback))]
        public static void DebugLogError(string message)
        {
            Debug.LogError("[c++] " + message);
        }
    }
}