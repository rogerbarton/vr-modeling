# C#/C++ Interface

*This page is quite techical, in most scenarios you can just look at what already exists!*

.. note:: 
	Native = C++, Managed = C# (think of memory management)

Quick Facts:
- Code written in C++ has to be compiled to a shared library (`.dll` on windows) and placed in the `Assets/Plugins` folder. This is done by CMake when building. 
- The native functions must be redeclared in C# as `extern` with the `DllImport` attribute and can then be called normally. All these declarations are in `Libigl/Native.cs`.
- You can use pointers with the `unsafe` keyword. 
- Managed data structures can have a different layout than native ones. *'Marshalling'* converts between the two automatically, but can involve expensive copies.
- Classes or structs can be shared between but must be declared in both languages.
- Beware of the garbage collector. Pin *managed* data using `fixed`, or `GcAlloc` with the pinned option, to prevent the garbage collector from moving/deleting whilst the C++ is executing. This is only for classes (not structs/value types). 

## C++ Building The Library

### CMake

- When building the `.dll` is placed in the `Assets/Plugins` folder automatically
- Note that the output directory can be set in the CMake cache with the `UNITY_*` variables
- Set `CMAKE_VERBOSE` for precise message if something goes wrong in CMake
- Currently only Visual Studio Solutions `.sln` have been tested

### Rebuilding and Unloading Native Libraries

Unity presents the complication that it *never unloads a dll once loaded*, this prevents write access and rebuilding will fail. A dll is loaded once a function from it is called for the first time.

[UnityNativeTool](https://github.com/mcpiroman/UnityNativeTool) by mcpiroman present a good workaround for this by *'mocking'* native functions and un/loading the dll manually. This is only done in the editor so builds will be unaffected.
This method allows us to use the normal P/Invoke attribute `[DllImport("mylib")]` above
external native function declarations in C#. So there are no changes to our code(!)
This works in edit and play mode and details can be seen in the instance of the `DllManipulatorScript`. However, this means the `Main` scene must be loaded in order to be able to use the dll.

We also get a callback whenever a library is loaded and unloaded (pre/post) allowing us to initialize
and clean up the native library nicely. This relied on the mysteriously named `stubLluiPlugin`.

### What you need to know:

- The library is **loaded whenever a function is called**, `Alt` + `Shift` + `D` is pressed
- It is **unloaded** when play mode ends, the `DllManipulatorScript` is disabled (`OnDisable`) or when manually unloading via the component inspector or the shortcut `Alt` + `D`
- **When you want to rebuild your library, stop play mode or unload it first in Unity via the shortcut**
- You can use `[DllImport]` as usual
- There are certain [limitations](https://github.com/mcpiroman/UnityNativeTool) to marshalling and similar
- We can get callbacks by using the attributes in `UnityNativeTool/scripts/Attributes.cs`, e.g. when the dll is un/loaded
- Use `[MockNativeDeclarations]` on a class or native function to enable this unloading
- The shortcuts un/load *all* mocked libraries, if there are several

## Debugging

**C++ or C#:** Open the solution in Visual Studio. `Debug > Attach To Process...` and select running `Unity.exe`. Place breakpoints as usual. Ensure that you build before running so that the source code matches the executing code. For C# debuggin in VS also search online...

**Simultaneous C#/C++:** VS cannot debug both at the same time, two instances do not work.
So current solution is to use Jetbrains Rider to debug the C# side and VS (or CLion) for C++.

.. tip::
   The editor/application will crash if there is a segfault in C++, use Visual Studio to debug.
   Failed assertions will cause a pop up. When this happens you can attach the debugger and
   then press `Retry` to inspect properly.

## Calling Native functions

### Do's and Don'ts

**Do:**

- Check that function **declarations match exactly** by copy-pasting for example
	- Be careful with references
- Label parameters with `in` and `out` to improve performance
- Use `unsafe` to pass pointers along with `UnsafeUtility`
- Use `NativeArray<T>` when possible along with `NativeArrayUnsafeUtility`
- Keep C#/C++ interface calls to a minimum for a simple interface

**Don't:**

- Pass large non-blittable types, e.g. matrices,  use pointers instead
- Have unhandled exceptions. Exceptions should be handled fully in C++ or fully in C#.
- Call a C# delegate/function pointer from C++ without checking if it is valid/null.

Lots of problems can arise if this is not the case.

## Global Variables/Persistent Memory in C++

Anything related to a specific mesh **must** be part of the :cpp:struct:`MeshState`. However, global variables can be used to store a state between function calls from C#.
Declare these as extern in a header and define them once in a C++. They can be set in the :cpp:func:`Initialize()`.

Memory allocated with `new` in C++ will persist as usual until it is deleted with `delete`. Notably, the :cpp:struct:`MeshState` is allocated in C++ when :cpp:func:`InitializeMesh` is called. C# can access (read/write but not delete) C++ owned memory.

.. note::
	When the dll is unloaded all memory it allocated must be deleted. This can be done in :cpp:func:`UnityPluginUnload` or triggered by a C# destructor, see `LibiglMesh.cs` and `LibiglBehaviour.cs`. Notably, when hot reloading this is also the case.

*(advanced)* When *hot reloading* (pausing play mode, un/loading the dll) global variables are deleted. Pointers to data allocated with `new` are still valid, but the memory cannot be used as the owner dll has been destroyed (effectively a segmentation fault). You cannot simply keep the same data. As such, all persistent data must be serialized and then deserialized if you want the state to survive a hot reload. This has not yet been implemented but could be done with `igl::serialize`.

## Marshalling

Marshalling allows us to pass managed data to a native context. Ensure that you use
['blittable types'](https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types) as much as possible as these do not involve a copy. Generally:

- blittable types: `int`, `float`, numbers, structs consisting of only these, 1-D arrays of these
- non-blittable types: `string`, `bool`, **n-D arrays**

To pass a struct add the `[StructLayout(Sequential)]` attribute to it in C# and redeclare it in C++ in `InterfaceTypes.h` with the *same variable ordering*. `in` and `out` parameter attributes allow the Marshalling to optimize more. It should match C++ references. For strings use `CharSet = Ansi` in `DllImport`

## Calling Managed functions from C++

.. note::
	In certain rare scenarios this may be desirable. Think first if this can be avoided. It is possible via function pointer callbacks. 

**In C#**, a delegate (~function pointer type) must be declared and the function to be called. The function must be annotated with
the `[MonoPInvokeCallback(typeof(MyDelegate))]` attribute. It must be a static method.   
See `Scripts/Libigl/NativeCallbacks.cs` and add your callback there.

**In C++**, declare a function pointer typedef like the delegate, see :cpp:type:`StringCallback`. The function pointer must use the :c:macro:`UNITY_INTERFACE_API` to ensure the `__stdcall` C# calling convention is used. Then you declare an instance of the function pointer as extern, see :cpp:member:`DebugLog`. Finally we must set the pointer when calling :cpp:func:`Initialize()` and reset to `nullptr` in :cpp:func:`UnityPluginUnload`. The extern variables need to be properly declared in `Native.cpp`.  
See `source/InterfaceTypes.h` and add your code there.

.. warning::
	Function pointers/callbacks may be invalid or null. Check before invoking them or a crash will occur.

Further reading: [Debug.Log example](https://answers.unity.com/questions/30620/how-to-debug-c-dll-code.html)

## Further Reading

A good [simple introduction to P/Invoke](https://manski.net/2012/06/pinvoke-tutorial-pinning-part-4/)

Unrelated and not what you want:

- C++/CLI (Microsoft) which is not the same as C++
- COM (Microsoft Component Object Model)
- CLR (Microsoft Common Language Runtime)

Related and what you are using/looking for:

- **P/Invoke** used by the `DllImportAttribute` (stands for Platform Invoke)

Links:

.. |fixedkw| replace:: ``fixed`` keyword
.. _fixedkw: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/fixed-statement

- |fixedkw|_
- [Mono Interop with Native Libraries, P/Invoke](https://www.mono-project.com/docs/advanced/pinvoke/)
- [Simple OpenCV Example](https://forum.unity.com/threads/tutorial-using-c-opencv-within-unity.459434/)
- [x86 Calling Conventions](https://en.wikipedia.org/wiki/X86_calling_conventions#stdcall) for `__stdcall`
- [Unity Macros](https://bitbucket.org/Unity-Technologies/graphicsdemos/src/buffer-ptr/NativeRenderingPlugin/PluginSource/source/Unity/IUnityInterface.h) only the first 20 lines
