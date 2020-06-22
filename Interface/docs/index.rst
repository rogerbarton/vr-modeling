Welcome to vr-modeling's documentation!
=======================================

*Deform meshes in Virtual Reality with libigl using Unity.*

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

.. TODO: Add gifs

#. Run As-Rigid-As-Possible ``igl::arap`` or a biharmonic defomation ``igl::harmonic`` on a mesh and manipulate it
   in real-time
#. Select vertices and transform them using a VR controller
#. Threaded geometry code, can handle armadillo with responsive VR
#. Multi-mesh editing possible
#. Multiple selections per mesh (using bitmasks)
#. Easy import process of new models
#. Easy UI generation

**Compatible VR Headsets:** Theoretically everything compatible with the Unity `XR Plugin <https://docs.unity3d.com/Manual/XR.html>`_
system and `XR Interaction Toolkit <https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html>`_.
Tested on Oculus Rift S, likely to be compatible are Oculus Rift, Oculus Quest, Valve Index, potentially HTC Vive.

Technical Features
^^^^^^^^^^^^^^^^^^

#. Unity C#/C++ interface
#. Unity/libigl interface for meshes
#. Handling of input with threaded geometry calls

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
