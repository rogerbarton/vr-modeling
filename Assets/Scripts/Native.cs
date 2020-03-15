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
        public const string DllName = "libigl-interface";

        public static VertexAttributeDescriptor[] VertexBufferLayout = new[]
        {
            //Note! Specify that the position is the only attribute in the first stream, else values will be interleaved
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3, 1)
            // new VertexAttributeDescriptor(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 1),
            // new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 1)
        };

        /// <summary>
        /// Setup callbacks inside the static constructor
        /// </summary>
        public static void Initialize()
        {
            Initialize(Application.dataPath + "/Models/", NativeCallbacks.DebugLog);
        }

        /// <summary>
        /// Clean up native part if required, called before unload dll
        /// </summary>
        public static void Destroy() { }

        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        private static extern void Initialize([In] string modelRootp,
            [In] NativeCallbacks.StringCallback debugCallback);

        [DllImport(DllName, ExactSpelling = true, CharSet = CharSet.Ansi)]
        public static extern unsafe void LoadOFF([In] string path, [In] float scale,
            [Out] out float* VPtr, [Out] out int VSize,
            [Out] out float* NPtr, [Out] out int NSize,
            [Out] out uint* FPtr, [Out] out int FSize);
        
        [DllImport(DllName)]
        public static extern unsafe void TranslateMesh([In] float* VPtr, [In] int VSize, [In, MarshalAs(UnmanagedType.Struct)]Vector3 value);
        // public static extern unsafe void TranslateMesh(
        //     [In,Out] float* VPtr, int VSize,
        //     [In, MarshalAs(UnmanagedType.Struct)] Vector3 value);
    }
}
