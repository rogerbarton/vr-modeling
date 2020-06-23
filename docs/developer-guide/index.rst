Developer Guide
===============

This is aimed at people wanting to view or edit the code.

The project is split into two subprojects, a Unity C# project and a C++ library.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   vr
   unity-mesh-api

Setup
^^^^^

**Required Tools**: Unity 2019.3.2f1+, CMake, Visual Studio, 'Desktop development with C++' workload in the Visual Studio installer.

**Recommended Tools (Optional):** JetBrains CLion (preferably 2020.1+) and Rider for C++ and C# development respectively. This has the benefit that you can debug C# and C++ simultaneously, which is not currenlty possible with Visual Studio.

**After Cloning:**

Checkout submodules: ``git submodule init && git submodule update``

Before opening Unity, setup the C++ interface to libigl with CMake in the `Interface` folder:

1. Run CMake inside the ``Interface`` C++ project, open the solution in **Visual Studio** and build
2. Or setup the CMake project in **CLion** and build, (ensure that the architecture is correct, e.g. ``x64``, in the Toolchain settings or you may have errors that the dll was not found).

Open the project in Unity. If you opened the project before building the library, you will need to Reimport the ``Assets/Models/EditableMeshes`` folder from the Right-Click menu in the project browser.

Press play in Unity and it should work.