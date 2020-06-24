# Developer Guide

.. note::
   
   This is aimed at people wanting to view or edit the code.
   The project is split into two subprojects, a Unity C# project and a C++ library.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   vr
   unity-mesh-api

## Setup

**Required Tools**: Unity 2019.3.2f1+, CMake, Visual Studio, 'Desktop development with C++' workload in the Visual Studio installer.

**Recommended Tools (Optional):** JetBrains CLion (preferably 2020.1+) and Rider for C++ and C# development respectively. This has the benefit that you can debug C# and C++ simultaneously, which is not currenlty possible with Visual Studio.

**After Cloning:**

Checkout submodules: `git submodule update --init`

Setup the C++ interface to libigl with CMake in the `Interface` folder:

1. Run CMake inside the `Interface` C++ project, open the solution in **Visual Studio** and build
2. Or setup the CMake project in **CLion** and build, (ensure that the architecture is correct, e.g. `x64`, in the Toolchain settings or you may have errors that the dll was not found).

Open the project in Unity.

1. Open the `Main` scene from the *Project* window.
2. *Reimport* the `Assets/Models/EditableMeshes` folder using the Right-Click menu.
3. (Optional) Go to the lighting window `Window` > `Rendering` > `Lighting Setting` and press `Generate Lighting` at the bottom.

Press play in Unity and it should work.

### Generating Documentation

To regenerate this documentation as well as the Doxygen documentation follow these steps. *Optional*

1. Install Doxygen

2. Run `pip install -r docs/requirements.txt` from the root directory in a terminal (cmd on windows)

    - You might have to restart for Sphinx to be found

3. Re-run CMake, this will create two new targets, build these like the library

   - **Doxygen**: Creates standard doxygen html/xml files. View this at `<cmake build folder>/docs/doxygen/index.html`
   - **Sphinx**: Creates the documentation as hosted on ReadTheDocs, using parts from the doxygen xml output.
     View locally at `<cmake build folder>/docs/sphinx/index.html` or push to master and then view online.