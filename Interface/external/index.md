C++ External Libraries
======================

Libigl among other third party libraries are in the ``external`` folder.

* ``eigen-debug``, stores natvis files for pretty printing Eigen matrices when debugging.
* ``UnityNativeTool``, this is the source code for the small ``stubLluiPlugin.dll`` library used by the UnityNativeTool.
  All it does is get the function pointer to the Unity C++ interface class ``IUnityInterfaces``.
  This is required such that we get the callbacks for :cpp:func:`UnityPluginLoad` and :cpp:func:`UnityPluginUnload` when
  running in the Unity editor. It is compiled via CMake.
* ``Unity/PluginAPI``, this has functions related calling Unity related functions from C++. Mostly unused.
* ``Unity/RenderAPI``, this includes headers related to sample usage of the render API (e.g. DirectX) from C++.
  It is currently not used.

.. toctree::
   :maxdepth: 2
   :caption: Contents:

   external-content
