# 3D Modeling in VR - Bachelor Thesis

## Setup

**Required Tools**: Unity 2019.3+, CMake, Visual Studio, 'Desktop development with C++' workload in the VS installer

**After Cloning:**
- Add the Oculus Integration to the project from the asset store
- (Optionally) add Odin Inspector to the project from the asset store
- Setup the C++ interface to libigl with CMake in the `Interface` folder
  1. Run CMake inside the Interface C++ project, open the solution in visual studio and build
  
## C++ Overview

Code written in C++ has to be compiled to a shared library (`.dll` on windows) and placed in the `Assets/Plugins` folder. This is done with a CMake project. The native functions are redeclared in C# as `extern` with the `DllImport` attribute and can then be called normally. 

You can pass pointers with the `unsafe` keyword. Be sure to pin managed data using `fixed` or `GcAlloc` with the pinned option to prevent the garbage collector from moving/deleting whilst the C++ is executing. Managed data structures can be different than native ones, *'Marshalling'* converts between the two automatically but can involve expensive copies.

*Note: Native = C++, Managed = C# (because of memory management)*

### Calling Native functions from C#

Marshalling allows us to pass managed data to a native context. Ensure that you use [*'blittable types'*](https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types) as much as possible as these do not involve a copy. Generally:

- blittable types: `int`, `float`, numbers, structs consisting of only these, 1-D arrays of these
- non-blittable types: `string`, `bool`, **n-D arrays**

To pass a struct add the `[StructLayout(Sequential)]` attribute to it in C# and redeclare it in C++ with the *same variable ordering*. In the native function declaration use `[MarshalAs(Struct)]`

`[In]` and `[Out]` parameter attributes allow the Marshalling to optimize more. These are separate from `in` and `out` which should match C++ references

Strings use `CharSet = Ansi` in `DllImport`

#### Do's and Don'ts

**Do:**

- Check that function **declarations match exactly** by copy-pasting for example, including references
- Label parameters with `[In]` and `[Out]` to improve performance
- Use `unsafe` to pass pointers along with `UnsafeUtility`
- Use `NativeArray<T>` when possible along `NativeArrayUnsafeUtility`
- Keep C#/C++ interface calls to a minimum

**Don't:**

- Pass large non-blittable types
- Have unhandled exceptions. Exceptions should be handled fully in C++ or fully in C#. Lots of problems can arise if this is not the case.

### Calling Managed functions from C++

Use delegates and function pointer with the `__stdcall` calling convention (which C# uses). These should be passed to the native `Initialize()` function

### Global Variables/Persistent Memory in C++

Use global variables to store a state between function calls from C#. Declare these as extern in a header and define them once in a C++. They can be set in the `Initialize()`

## C++ Building The Library

### CMake

- When building the `.dll` is placed in the `Assets/Plugins` folder automatically
- Note that the output directory can be set in the CMake cache with the `UNITY_*` variables
- Set `CMAKE_VERBOSE` for precise message if something goes wrong in CMake
- Currently only Visual Studio Solutions `.sln` have been tested

### Rebuilding and Unloading Native Libraries

Unity presents the complication that it *never unloads a dll once loaded*, this prevents write access and rebuilding will fail. A dll is loaded once a function from it is called for the first time. 

[UnityNativeTool](https://github.com/mcpiroman/UnityNativeTool) by mcpiroman present a good workaround for this by *'mocking'* native functions and un/loading the dll manually. This is only done in the editor so builds will be unaffected. This method allows us to use the normal P/Invoke attribute `[DllImport("mylib")]` above external native function declarations in C#. So there are no changes to our code. This works in edit and play mode and details can be seen in the instance of the `DllManipulatorScript`.

We also get a callback whenever a library is loaded and unloaded (pre/post) allowing us to initialize and clean up the native library nicely.

#### What you need to know:

- The library is **loaded whenever a function is called**, `Alt`+`Shift`+`W` is pressed
- It is **unloaded** when play mode ends, the `DllManipulatorScript` is disabled (`OnDisable`) or when manually unloading via the component inspector or the shortcut `Alt`+`W`
- You can use `[DllImport]` as usual 
- There are certain [limitations](https://github.com/mcpiroman/UnityNativeTool) to marshalling and similar
- **When you want to rebuild your library, unload it first in Unity via the shortcut**
- The `DllCallbacks.cs` file can be customized with `OnDllLoaded`, `OnBeforeDllUnload` and `OnAfterDllUnload`
- Use `[MockNativeDeclarations]` on a class or native function to enable this unloading
- The shortcuts un/load *all* mocked libraries, if there are several

### Debugging

**C++:** Open the solution in visual studio. `Debug > Attach To Process...` and select running Unity.exe
Place breakpoints as usual

**Simultaneous C#:** VS cannot debug both at the same time, two instances do not work. 
So current solution is to use Jetbrains Rider to debug the C# side and VS for C++.

 Notes:

  - The editor/application will crash if there is a segfault in C++, use VS to debug.

### Further Reading

Unrelated and not what you want:

- C++/CLI (Microsoft) which is not the same as C++
- COM (Microsoft Component Object Model)
- CLR (Microsoft Common Language Runtime)

Related and what you are using/looking for:

- **P/Invoke** used by the `DllImportAttribute`

Links:

- [`fixed` keyword](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/fixed-statement)
- [Mono Interop with Native Libraries, P/Invoke](https://www.mono-project.com/docs/advanced/pinvoke/)
- [Simple OpenCV Example](https://forum.unity.com/threads/tutorial-using-c-opencv-within-unity.459434/)
- [x86 Calling Conventions](https://en.wikipedia.org/wiki/X86_calling_conventions#stdcall) for `__stdcall`
- [Unity Macros](https://bitbucket.org/Unity-Technologies/graphicsdemos/src/buffer-ptr/NativeRenderingPlugin/PluginSource/source/Unity/IUnityInterface.h) only the first 20 lines

## Mesh API Overview

- Since Unity 2019.3 there have been some updates to the mesh API. 
- `mesh.MarkDynamic()` on a *readable* mesh to keep a copy of the buffers on the managed CPU memory and make `mesh.vertices` read/writable. 
- `mesh.UploadData(false)` to copy the `mesh.vertices` managed data to the GPU immediately (else done pre-render), using `true` will delete the CPU copy.
- Get GPU (DirectX) pointer with `mesh.GetNativeVertexBufferPtr()`
- `mesh.SetVertexBufferParams()` to specify the layout on the GPU, attributes in the same stream are interleaved.
  - `RecalculateNormals` or `RecalculateTangents` do not seem to work when the normals/tangents are not in stream 0

### Further Reading

1. [Unity Docs - Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html)
2. [Unity Docs - Mesh.SetVertexBufferData](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferData.html)
3. [Unity Docs - Mesh.SetVertexBufferParams](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferParams.html)
4. [Sample from GraphicsDemos](https://bitbucket.org/Unity-Technologies/graphicsdemos/pull-requests/2/example-of-native-vertex-buffers-for/diff)
5. [Mesh API Feedback Forum](https://forum.unity.com/threads/feedback-wanted-mesh-scripting-api-improvements.684670/) with link to [Google Docs](https://docs.google.com/document/d/1I225X6jAxWN0cheDz_3gnhje3hWNMxTZq3FZQs5KqPc/edit)
6. [`NativeArray<T>` use cases](https://gamedev.stackexchange.com/questions/174953/unity-uses-for-nativearray/174956#174956?newreg=ee4ce68f58c540479161bad1841be246)