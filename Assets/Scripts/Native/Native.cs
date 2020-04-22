using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityNativeTool;

namespace libigl
{
    /// <summary>
    /// Contains all runtime C++ function declarations.
    /// These functions can only be called in play mode, as the dll is unloaded otherwise for easier rebuilds.
    /// C# to C++ Communication with marshalling attributes
    /// </summary>
    [MockNativeDeclarations] // Use UnityNativeTool to un/load dll functions in this class
    public static class Native
    {
        public const string DllName =
#if UNITY_EDITOR
            "__libigl-interface";
#else
            "__libigl-interface";
#endif

        public static VertexAttributeDescriptor[] VertexBufferLayout = {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 0),
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0)
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        };

        #if !UNITY_EDITOR
        // In a build Initialize in the static ctor.
        // This is called once just before the first function is called in this class.
        static Native() { Initialize(); }
        #endif
        
        /// <summary>
        /// Initializes the native library and sets up callbacks/delegates for C++ -> C# calls.
        /// Note: this may not be called on the main thread. So Unity functions may not be available
        /// </summary>
        #if UNITY_EDITOR
        // [NativeDllLoadedTrigger] //Trigger this each time the dll is loaded, so we reinitialize if we reload it
        #endif
        public static void Initialize()
        {
            Initialize(NativeCallbacks.DebugLog);
        }

        /// <summary>
        /// Clean up native part if required, called before unload dll
        /// </summary>
        public static void Destroy() { }

        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize([In] NativeCallbacks.StringCallback debugCallback);

        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern unsafe void LoadOFF([In] string path, [In] float scale,
            [Out] out float* VPtr, [Out] out int VSize,
            [Out] out float* NPtr, [Out] out int NSize,
            [Out] out uint* FPtr, [Out] out int FSize);
        
        
        [DllImport(DllName)]
        public static extern unsafe void TransposeInPlace(void* MatrixPtr, int rows);

        [DllImport(DllName)]
        public static extern unsafe void TranslateMesh([In,Out] void* VPtr, [In] int VSize, [In, MarshalAs(UnmanagedType.Struct)]Vector3 value);
        
        [DllImport(DllName)]
        public static extern unsafe void Harmonic([In,Out] void* VPtr, [In] int VSize, [In] void* FPtr, [In] int FSize);
    }
}
