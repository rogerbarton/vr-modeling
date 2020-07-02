# C# Scripts Overview

The important overall picture is covered here, for more detailed notes about the implementation see the code itself, `Crl` + `Q` with Rider or generate the doxygen documentation locally with CMake.

In the `Scripts` folder, the subfolders correspond to the namespaces. Beware that `Editor` folders are treated specially by Unity and will not be included in a build.

- **Libigl** - This is where most of the interesting code is: modifying/updating the geometry, making calls to C++, threading, anything related to the meshes. 
   - **Editor** - Scripts related to importing and pre-processing models so that we can modify them with libigl.
- **XrInput** - This is where interface with the controllers is, reading values, setting up controllers when detected, note setting up tracking is done in the scene with the XR Interaction Toolkit components
- **UI** - This is about the 2D user interfaces. How to generate the UI panels, update them and defining the click behaviour.
    - **Components** - This contains lots of smaller scripts to define the behaviour of a generated component. See `Assets/Prefabs/UI`.
    - **Hints** - Behaviour and data related to displaying 2D UI hints over the controllers to show what each button does. This is quite context sensitive for each tool and sub-state. 
    Uses *ScriptableObjects* so that we can enter the data in the Unity Editor.
- **Util** - Some helper scripts 
- **Testing** - Scripts here are not used but might be of interest to see how the Unity APIs work (particularly the mesh API).

.. warning::
   **The C# code documentation is *experimental*.** There is lots of duplication and some parts are hidden. If something is displayed completely incorrectly, please create an issue so it can be fixed. However, this is hopefully still useful.
   You can always see the **doxygen** documentation locally or **view the code comments** themselves in the IDE.

## Libigl Overview

.. .. doxygennamespace:: Libigl
   :no-link:

## XrInput Overview

.. .. doxygennamespace:: XrInput
   :no-link:

## UI Overview

.. .. doxygennamespace:: UI
   :no-link: