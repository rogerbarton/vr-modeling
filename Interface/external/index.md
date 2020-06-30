# C++ External Libraries

Libigl among other third party libraries are in the `external` folder.

- `libigl`, This is currently a custom fork with some modifications. It also contains Eigen.
    - `arap*` files have been edited to allow for use of `float` instead of `double`.
- `eigen-debug`, Stores natvis files for pretty printing Eigen matrices when debugging.
- `UnityNativeTool`, This is the source code for the small `stubLluiPlugin.dll` library used by the UnityNativeTool.
  All it does is get the function pointer to the Unity C++ interface class `IUnityInterfaces`.
  This is required such that we get the callbacks for :cpp:func:`UnityPluginLoad` and :cpp:func:`UnityPluginUnload` when
  running in the Unity editor. It is compiled via CMake.
- `Unity/PluginAPI`, This has functions related calling Unity related functions from C++. Mostly unused.
- `Unity/RenderAPI`, This includes headers related to sample usage of the render API (e.g. DirectX) from C++.
  It is currently not used.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   external-content
