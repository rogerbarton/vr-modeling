using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityNativeTool;

namespace Libigl
{
    /// <summary>
    /// Contains all C++ function declarations.
    /// Handles initialization of the DLL and works with the UnityNativeTool for easy reloading/recompilation.
    /// </summary>
    /// <remarks>Convention: Pass the <see cref="MeshState"/> as the first argument if the function modifies a mesh.</remarks>
    [MockNativeDeclarations] // Use UnityNativeTool to un/load dll functions in this class
    public static class Native
    {
        /// <summary>
        /// Name of the dll, done so that in the editor we can use the UnityNativeTool.
        /// </summary>
        public const string DllName =
#if UNITY_EDITOR
            "libigl-interface"; //TODO: invert this so Unity searches for __lib, but we actually have lib in folders
#else
            "__libigl-interface";
#endif

        /// <summary>
        /// Contains the vertex buffer layout for editable meshes. This was initially intended to be done for more
        /// performant applying of mesh data but allows for slighly more control over how meshes are stored on the GPU.
        /// </summary>
        public static VertexAttributeDescriptor[] VertexBufferLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1),
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 3, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 3), // UV
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.UInt32, 1,
                2) // Selection coupled with Color
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
        /// Clean up native part if required, called just *before* unloading of the dll
        /// </summary>
        public static void Destroy()
        {
        }

        #region Native Function Redeclarations

        // This is where we redeclare the exported C++ functions. This needs to match the C++ exactly.

        // Interface.cpp
        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize(NativeCallbacks.StringCallback debugCallback,
            NativeCallbacks.StringCallback debugWarningCallback, NativeCallbacks.StringCallback debugErrorCallback);

        [DllImport(DllName)]
        public static extern unsafe MeshState* InitializeMesh([In, Out] UMeshDataNative data, string name);

        [DllImport(DllName)]
        public static extern unsafe void DisposeMesh(MeshState* data);


        // IO.cpp
        [DllImport(DllName)]
        public static extern unsafe void ApplyDirty(MeshState* state, UMeshDataNative data, uint visibleSelectionMask);

        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern unsafe void ReadOFF(string path, bool setCenter, bool normalizeScale,
            float scale,
            out float* VPtr, out int VSize,
            out float* NPtr, out int NSize,
            out uint* FPtr, out int FSize,
            bool calculateNormalsIfEmpty);


        // ModifyMesh.cpp
        [DllImport(DllName)]
        public static extern unsafe void TranslateAllVertices(MeshState* state, Vector3 value);

        [DllImport(DllName)]
        public static extern unsafe void TranslateSelection(MeshState* state, Vector3 value, uint maskId);

        [DllImport(DllName)]
        public static extern unsafe void TransformSelection(MeshState* state, Vector3 translation, float scale,
            Quaternion rotation, Vector3 pivot, uint maskId);

        [DllImport(DllName)]
        public static extern unsafe void Harmonic(MeshState* state, uint boundaryMask, bool showDeformationField);

        [DllImport(DllName)]
        public static extern unsafe void Arap(MeshState* state, uint boundaryMask);

        [DllImport(DllName)]
        public static extern unsafe void ResetV(MeshState* state);


        // Selection.cpp
        [DllImport(DllName)]
        public static extern unsafe void SelectSphere(MeshState* state, Vector3 position, float radius,
            int selectionId, uint selectionMode);

        [DllImport(DllName)]
        public static extern unsafe uint GetSelectionMaskSphere(MeshState* state, Vector3 position, float radius);

        [DllImport(DllName)]
        public static extern unsafe Vector3 GetSelectionCenter(MeshState* state, uint maskId);

        [DllImport(DllName)]
        public static extern unsafe void ClearSelectionMask(MeshState* state, uint maskId);

        [DllImport(DllName)]
        public static extern unsafe void SetColorSingleByMask(MeshState* state, uint maskId, int colorId);

        [DllImport(DllName)]
        public static extern unsafe void SetColorByMask(MeshState* state, uint maskId);

        #endregion
    }
}