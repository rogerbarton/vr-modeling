using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityNativeTool;

namespace libigl
{
    /// <summary>
    /// Contains all C++ function declarations for the editor dll.
    /// These functions can be called in edit or play mode.
    /// The editor has to be closed to rebuild the dll for these functions.
    /// </summary>
    [DisableMocking] //Disable unloading, let Unity load these functions
    public static class NativeEditor
    {
        private const string dllName = "libigl-editor";
        private static bool initialized = false;

        /// <summary>
        /// Call this to ensure that the native plugin has been properly initialized, callbacks set up.
        /// </summary>
        public static void Initialize()
        {
            if (!initialized)
                Initialize(NativeCallbacks.DebugLog);
            initialized = true;
        }

        [DllImport(dllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize([In] NativeCallbacks.StringCallback debugCallback);

        [DllImport(dllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern unsafe void LoadOFF([In] string path, [In] float scale,
            [Out] out float* VPtr, [Out] out int VSize,
            [Out] out float* NPtr, [Out] out int NSize,
            [Out] out uint* FPtr, [Out] out int FSize);

    }
}