Welcome to vr-modeling's documentation!
=======================================

*Deform meshes in virtual reality with libigl using Unity.*

.. toctree::
   :maxdepth: 2
   :caption: Contents:
   :numbered:

   self
   user-guide/index
   developer-guide/index
   cs-reference/index
   cpp-reference/index

* See whole :ref:`genindex`

Features
^^^^^^^^

TODO: Add gifs

#. Run As-Rigid-As-Possible ``igl::arap`` or a biharmonic defomation ``igl::harmonic`` on a mesh and manipulate it
   in real-time
#. Select vertices and transform them using a VR controller
#. Threaded geometry code, can handle armadillo with responsive VR
#. Multi-mesh editing possible
#. Multiple selections per mesh (using bitmasks)
#. Easy import process of new models
#. Easy UI generation

**Compatible VR Headsets:** Theoretically everything compatible with the Unity [XR Plugin](https://docs.unity3d.com/Manual/XR.html)
system and [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html).
Tested on Oculus Rift S, likely to be compatible are Oculus Rift, Oculus Quest, Valve Index, potentially HTC Vive.

Technical Features
^^^^^^^^^^^^^^^^^^

#. Unity C#/C++ interface
#. Unity/libigl interface for meshes
#. Handling of input with threaded geometry calls

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

Development Timeline
^^^^^^^^^^^^^^^^^^^^
.. raw:: html

   <iframe allowfullscreen src='https://timelines.gitkraken.com/timeline/c1c573c02b5749eca69a3107f3b57999?showControlPanel=true&showMinimap=true&allowPresentationMode=true' style='width:100%;height:500px;border:none;'></iframe>

Project Overview
^^^^^^^^^^^^^^^^

Important folders:

- `Assets` - Unity related files

   - `Scripts` - C# code for things like: UI, Input, Unity mesh interface, Threading, Importing models
   - `Prefabs` - Pre-made UI components
   - `Models/EditableMeshes` - The meshes that can be used in VR
   - `Materials` - Textures, icons and shaders
- `Interface` - C++ project that interfaces with libigl: deformations, modifying meshes via eigen matrices

   - `source` - the C++ source code which calls libigl
