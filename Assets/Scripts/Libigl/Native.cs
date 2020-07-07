using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityNativeTool;

namespace Libigl
{
    /// <summary>
    /// Contains all C++ function declarations. <b>C# to C++</b><p>
    /// Handles initialization of the DLL and works with the UnityNativeTool for easy reloading/recompilation.</p>
    /// </summary>
    /// <remarks>Convention: Pass the <see cref="MeshState"/> as the first argument if the function modifies a mesh.</remarks>
    [MockNativeDeclarations] // Use UnityNativeTool to un/load dll functions in this class
    public static class Native
    {
        /// <summary>
        /// Name of the dll without the extension. Use this with the <see cref="DllImportAttribute"/>.
        /// This is set such that in the editor we can use the UnityNativeTool and in the build we use the library directly.
        /// </summary>
        /// <remarks>
        /// In Editor: <c>libigl-interface</c><p>
        /// In Build and Actual Name: <c>__libigl-interface</c></p>
        /// </remarks>
        private const string DllName =
#if UNITY_EDITOR
            "libigl-interface"; //TODO: invert this so Unity searches for __lib, but we actually have lib in folders
#else
            "__libigl-interface";
#endif

        /// <summary>
        /// Contains the vertex buffer layout (on the GPU) for editable meshes.
        /// There will be a copy of the mesh on the CPU which may not have the same layout.
        /// This was initially intended to be done for more a efficient applying of mesh data,
        /// but it allows for slightly more control over how meshes are stored on the GPU.
        /// </summary>
        public static readonly VertexAttributeDescriptor[] VertexBufferLayout = new[]
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
        /// <summary>
        /// In a build, Initialize in the static ctor.
        /// This is called once just before the first function is called in this class.
        /// </summary>
        /// <remarks>Please read up on static constructors before modifying this.</remarks>
        static Native() { Initialize(); }
#endif

        /// <summary>
        /// Initializes the native library and sets up callbacks/delegates for C++ -> C# calls.
        /// Note: this may not be called on the main thread. So Unity functions may not be available.
        /// </summary>
        /// <remarks>In the editor, this is triggered <b>each time</b> the dll has been loaded.</remarks>
#if UNITY_EDITOR
        [NativeDllLoadedTrigger] // Trigger this each time the dll is loaded, so we reinitialize if we reload it
#endif
        public static void Initialize()
        {
            // TODO: Check which library has been unloaded
            Initialize(NativeCallbacks.DebugLog, NativeCallbacks.DebugLogWarning, NativeCallbacks.DebugLogError);
        }

        /// <summary>
        /// Clean up native part if required, called <b>just before</b> unloading of the dll.
        /// </summary>
        [NativeDllBeforeUnloadTrigger]
        public static void Destroy()
        {
        }

        #region Native Function Redeclarations

        // This is where we redeclare the exported C++ functions.
        // This needs to match the C++ exactly, translated to C# of course.
        // Use the keyword 'ref' for C++ references

        // Native.cpp
        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize(NativeCallbacks.StringCallback debugCallback,
            NativeCallbacks.StringCallback debugWarningCallback, NativeCallbacks.StringCallback debugErrorCallback);

        [DllImport(DllName)]
        public static extern unsafe MeshState* InitializeMesh(UMeshDataNative data, string name);

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