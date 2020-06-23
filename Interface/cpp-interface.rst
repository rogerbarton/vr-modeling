C#/C++ Interface
================

Code written in C++ has to be compiled to a shared library (``.dll`` on windows) and placed in the ``Assets/Plugins`` folder.
This is done with a CMake project. The native functions are redeclared in C# as ``extern`` with the ``DllImport`` attribute
and can then be called normally.

You can pass pointers with the ``unsafe`` keyword. Be sure to pin managed data using ``fixed`` or ``GcAlloc`` with the pinned
option to prevent the garbage collector from moving/deleting whilst the C++ is executing. Managed data structures can be
different than native ones, *'Marshalling'* converts between the two automatically but can involve expensive copies.

*Note: Native = C++, Managed = C# (think of memory management)*

Calling Native functions from C#
--------------------------------

Marshalling allows us to pass managed data to a native context. Ensure that you use
`'blittable types' <https://docs.microsoft.com/en-us/dotnet/framework/interop/blittable-and-non-blittable-types>`_ as
much as possible as these do not involve a copy. Generally:

- blittable types: ``int``, ``float``, numbers, structs consisting of only these, 1-D arrays of these
- non-blittable types: ``string``, ``bool``, **n-D arrays**

To pass a struct add the ``[StructLayout(Sequential)]`` attribute to it in C# and redeclare it in C++ with the
*same variable ordering*. In the native function declaration use ``[MarshalAs(Struct)]``

``[In]`` and ``[Out]`` parameter attributes allow the Marshalling to optimize more.
These are separate from ``in`` and ``out`` which should match C++ references

Strings use ``CharSet = Ansi`` in ``DllImport``

Do's and Don'ts
^^^^^^^^^^^^^^^

**Do:**

- Check that function **declarations match exactly** by copy-pasting for example, including references
- Label parameters with ``[In]`` and ``[Out]`` to improve performance
- Use ``unsafe`` to pass pointers along with ``UnsafeUtility``
- Use ``NativeArray<T>`` when possible along ``NativeArrayUnsafeUtility``
- Keep C#/C++ interface calls to a minimum

**Don't:**

- Pass large non-blittable types
- Have unhandled exceptions. Exceptions should be handled fully in C++ or fully in C#.
- Call a C# delegate/function pointer from C++ without checking if it is valid/null.

Lots of problems can arise if this is not the case.

Calling Managed functions from C++
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Use delegates and function pointer with the ``__stdcall`` calling convention (which C# uses).
These should be passed to the native ``Initialize()`` function

Global Variables/Persistent Memory in C++
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

Use global variables to store a state between function calls from C#.
Declare these as extern in a header and define them once in a C++. They can be set in the ``Initialize()``

C++ Building The Library
========================

CMake
-----

- When building the ``.dll`` is placed in the ``Assets/Plugins`` folder automatically
- Note that the output directory can be set in the CMake cache with the ``UNITY_*`` variables
- Set ``CMAKE_VERBOSE`` for precise message if something goes wrong in CMake
- Currently only Visual Studio Solutions ``.sln`` have been tested

Rebuilding and Unloading Native Libraries
-----------------------------------------

Unity presents the complication that it *never unloads a dll once loaded*,
this prevents write access and rebuilding will fail. A dll is loaded once a function from it is called for the first time.

`UnityNativeTool <https://github.com/mcpiroman/UnityNativeTool>`_ by mcpiroman present a good workaround for this
by *'mocking'* native functions and un/loading the dll manually. This is only done in the editor so builds will be unaffected.
This method allows us to use the normal P/Invoke attribute ``[DllImport("mylib")]`` above
external native function declarations in C#. So there are no changes to our code.
This works in edit and play mode and details can be seen in the instance of the ``DllManipulatorScript``.

We also get a callback whenever a library is loaded and unloaded (pre/post) allowing us to initialize
and clean up the native library nicely.

What you need to know:
^^^^^^^^^^^^^^^^^^^^^^

- The library is **loaded whenever a function is called**, ``Alt`` + ``Shift`` + ``D`` is pressed
- It is **unloaded** when play mode ends, the ``DllManipulatorScript`` is disabled (``OnDisable``) or
  when manually unloading via the component inspector or the shortcut ``Alt`` + ``D``
- You can use ``[DllImport]`` as usual
- There are certain `limitations <https://github.com/mcpiroman/UnityNativeTool>`_ to marshalling and similar
- **When you want to rebuild your library, unload it first in Unity via the shortcut**
- The ``DllCallbacks.cs`` file can be customized with ``OnDllLoaded``, ``OnBeforeDllUnload`` and ``OnAfterDllUnload``
- Use ``[MockNativeDeclarations]`` on a class or native function to enable this unloading
- The shortcuts un/load *all* mocked libraries, if there are several

Debugging
---------

**C++:** Open the solution in visual studio. ``Debug > Attach To Process...`` and select running Unity.exe
Place breakpoints as usual

**Simultaneous C#/C++:** VS cannot debug both at the same time, two instances do not work.
So current solution is to use Jetbrains Rider to debug the C# side and VS (or CLion) for C++.

.. tip::
   The editor/application will crash if there is a segfault in C++, use Visual Studio to debug.
   Failed assertions will cause a pop up. When this happens you can attach the debugger and
   then press `Retry` to inspect properly.

Further Reading
---------------

A good `simple introduction to P/Invoke <https://manski.net/2012/06/pinvoke-tutorial-pinning-part-4/>`_

Unrelated and not what you want:

- C++/CLI (Microsoft) which is not the same as C++
- COM (Microsoft Component Object Model)
- CLR (Microsoft Common Language Runtime)

Related and what you are using/looking for:

- **P/Invoke** used by the ``DllImportAttribute`` (stands for Platform Invoke)

Links:

.. |fixedkw| replace:: ``fixed`` keyword
.. _fixedkw: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/fixed-statement

- |fixedkw|_
- `Mono Interop with Native Libraries, P/Invoke <https://www.mono-project.com/docs/advanced/pinvoke/>`_
- `Simple OpenCV Example <https://forum.unity.com/threads/tutorial-using-c-opencv-within-unity.459434/>`_
- `x86 Calling Conventions <https://en.wikipedia.org/wiki/X86_calling_conventions#stdcall>`_ for ``__stdcall``
- `Unity Macros <https://bitbucket.org/Unity-Technologies/graphicsdemos/src/buffer-ptr/NativeRenderingPlugin/PluginSource/source/Unity/IUnityInterface.h>`_ only the first 20 lines

Known Issues
------------

1. Using both debug and release CMake profiles in CLion does not work. Profiles are loaded in parallel which causes issues with cloning Eigen.
   Use only one profile at a time.
   `CLion Issue <https://youtrack.jetbrains.com/issue/CPP-20496?_ga=2.34925026.276428072.1590419347-85201278.1567248252&_gac=1.250350196.1587037749.CjwKCAjwhOD0BRAQEiwAK7JHmGREcAuH_f0dFLzdf_CEwVvREfHYy-2HZWvdkxfXeSXVuiuojkqZ1RoCimEQAvD_BwE>`_