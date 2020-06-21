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

.. note::

   :c:macro:`UNITY_INTERFACE_EXPORT` is a macro provided by Unity in ``external/Unity/IUnityInterface.h``,
   which allows the function to be callable from C# (given it is within an ``extern "C"`` clause)

.. doxygenfile:: InterfaceTypes.h


ModifyMesh.h
^^^^^^^^^^^^

This is where the deformations are as well as other functions which manipulate the vertices.
This is a good place to start for how to implement your own deformation.

.. doxygenfile:: ModifyMesh.h

State.h
^^^^^^^

This is the shared state between C++/C# and changes in one **must** be applied to the other.
If the two structs do not match *exactly* problems arise with reading/writing to the wrong memory.

.. doxygenfile:: State.h

NativeState.h
^^^^^^^^^^^^^

This contains data, specific to a mesh, for storing data only used in C++.

.. doxygenfile:: NativeState.h

Util.h
^^^^^^

Contains various helper functions, classes and constants.

.. doxygenfile:: Util.h


