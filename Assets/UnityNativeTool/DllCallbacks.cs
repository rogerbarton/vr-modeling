﻿using libigl;
using UnityEngine;

namespace UnityNativeTool
{
    /// <summary>
    /// A place to implement you custom callbacks.
    /// </summary>
    public static class DllCallbacks
    {
        /// <summary>
        /// This is called whenever a dll is loaded.
        /// </summary>
        /// <param name="dllName">The name without preceding underscores or file extension.</param>
        public static void OnDllLoaded(string dllName)
        {
            Debug.Log("[dll] " + dllName + " dll loaded.");
            if (dllName == Native.DllName)
                Native.Initialize();
        }

        
        /// <summary>
        /// This is called whenever a dll is unloaded.
        /// </summary>
        /// <param name="dllName">The name without preceding underscores or file extension.</param>
        public static void OnBeforeDllUnload(string dllName)
        {
            // Debug.Log("[dll] " + dllName + " dll unloading...");
            if (dllName == Native.DllName)
                Native.Destroy();
        }
        
        /// <summary>
        /// This is called whenever a dll is unloaded.
        /// </summary>
        /// <param name="dllName">The name without preceding underscores or file extension.</param>
        public static void OnAfterDllUnload(string dllName)
        {
            Debug.Log("[dll] " + dllName + " dll unloaded.");
        }
    }
}