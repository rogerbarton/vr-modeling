using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityNativeTool;

namespace Libigl
{
    /// <summary>
    /// Contains all runtime C++ function declarations.
    /// These functions can only be called in play mode, as the dll is unloaded otherwise for easier rebuilds.
    /// C# to C++ Communication with marshalling attributes
    /// <remarks>Convention: Pass the <see cref="State"/> as the first argument if the function modifies a mesh.</remarks>
    /// </summary>
    [MockNativeDeclarations] // Use UnityNativeTool to un/load dll functions in this class
    public static class Native
    {
        public const string DllName =
#if UNITY_EDITOR
            "libigl-interface"; //TODO: invert this so Unity searches for __lib, but we actually have lib in folders
#else
            "__libigl-interface";
#endif

        public static VertexAttributeDescriptor[] VertexBufferLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 3, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 3), // UV
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.UInt32, 1, 2) // Selection coupled with Color
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
        [NativeDllLoadedTrigger] //Trigger this each time the dll is loaded, so we reinitialize if we reload it
#endif
        public static void Initialize()
        {
            Initialize(NativeCallbacks.DebugLog, NativeCallbacks.DebugLogWarning, NativeCallbacks.DebugLogError);
        }

        /// <summary>
        /// Clean up native part if required, called before unload dll
        /// </summary>
        public static void Destroy() { }

        // Interface.cpp
        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize(NativeCallbacks.StringCallback debugCallback,
            NativeCallbacks.StringCallback debugWarningCallback, NativeCallbacks.StringCallback debugErrorCallback);

        [DllImport(DllName)]
        public static extern unsafe State* InitializeMesh([In, Out] UMeshDataNative data, string name);

        [DllImport(DllName)]
        public static extern unsafe void DisposeMesh(State* data);


        // IO.cpp
        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern unsafe void LoadOFF([In] string path, [In] bool setCenter, [In] bool normalizeScale,
            [In] float scale,
            [Out] out float* VPtr, [Out] out int VSize,
            [Out] out float* NPtr, [Out] out int NSize,
            [Out] out uint* FPtr, [Out] out int FSize);

        [DllImport(DllName)]
        public static extern unsafe void ApplyDirty(State* state, UMeshDataNative data, uint visibleSelectionMask);


        // ModifyMesh.cpp
        [DllImport(DllName)]
        public static extern unsafe void TranslateMesh(State* state, Vector3 value);

        [DllImport(DllName)]
        public static extern unsafe void TranslateSelection(State* state, Vector3 value, int selectionId);

        [DllImport(DllName)]
        public static extern unsafe void TransformSelection(State* state, int selectionId,
            Vector3 translation, float scale, Quaternion rotation);

        [DllImport(DllName)]
        public static extern unsafe void Harmonic(State* state, uint boundaryMask, bool showDeformationField);
        
        [DllImport(DllName)]
        public static extern unsafe void Arap(State* state, uint boundaryMask);
        
        [DllImport(DllName)]
        public static extern unsafe void ResetV(State* state);


        // Selection.cpp
        [DllImport(DllName)]
        public static extern unsafe void SelectSphere(State* state, Vector3 position, float radius,
            int selectionId, uint selectionMode);
        
        [DllImport(DllName)]
        public static extern unsafe uint GetSelectionMaskSphere(State* state, Vector3 position, float radius);

        [DllImport(DllName)]
        public static extern unsafe void ClearSelection(State* state, int selectionId);

        [DllImport(DllName)]
        public static extern unsafe void ClearSelectionMask(State* state, uint maskId);

        [DllImport(DllName)]
        public static extern unsafe void SetColorBySelection(State* state, int selectionId);

        [DllImport(DllName)]
        public static extern unsafe void SetColorByMask(State* state, uint maskId);
    }
}
