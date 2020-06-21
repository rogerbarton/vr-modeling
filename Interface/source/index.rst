C++ API Reference
=================

Interface.h
^^^^^^^^^^^

This is the central file with all exported functions that are callable from C#. These are the 'entry points'.
As this is a library we do not have a ``main()``, however we have the :cpp:func:`Initialize` function as a replacement.
Unity also triggers the functions ``UnityPluginLoad()`` and ``UnityPluginUnload()`` at the start and end.
These are called first and last, notably :cpp:func:`Initialize` is called after ``UnityPluginLoad()``.

The two important functions for the lifecycle of a mesh are :cpp:func:`InitializeMesh` and :cpp:func:`DisposeMesh`.
These are called whenever a :cpp:class:`LibiglMesh` is instaniated or destroyed in Unity. This is where the C++ owned
memory is allocated and deleted.

To make these functions callable from C# we must put the declarations inside an ``extern "C"`` scope,
as well as prepending the :c:macro:`UNITY_INTERFACE_EXPORT` to the declaration. This is because C# and C++ use different
default calling conventions, see `x86 Calling Conventions <https://en.wikipedia.org/wiki/X86_calling_conventions#stdcall>`_
specifically ``__stdcall``. *This also is different for every platform, but luckily the IUnityInterface header handles
this for us if we use this macro.*

.. doxygenfile:: Interface.h

InterfaceTypes.h
^^^^^^^^^^^^^^^^^

This file includes all the types that are shared between C# and C++.
These are declared once in both languages and if one if modified the other must also be updated.
In C# this corresponds to the classes with the attribute ``[StructLayout(LayoutKind.Sequential)]``.
The :ref:`State` is also an interface type but has its own file.

Note that :c:macro:`UNITY_INTERFACE_EXPORT` is a macro provided by Unity in ``external/Unity/IUnityInterface.h``,
which allows the function to be callable from C# (given it is within an ``extern "C"`` clause)

.. doxygenfile:: InterfaceTypes.h
