Unity C++ Plugin API
====================

These files contain the (quite limited) Unity C++ API for plugins and is only really intended for special cases.
These files are available from any Unity installation. They are located in ``<Unity Install Dir>/Editor/Data/PluginAPI``
, e.g. ``C:\Program Files\Unity\Hub\Editor\2019.3.2f1\Editor\Data\PluginAPI``.

.. warning::
	Comments here mainly reflect *my understanding* of the API!
	This part is just for reference, but if you really intend on using this you need to look at the source code
	(which also	has a lot more comments).

IUnityInterface.h
^^^^^^^^^^^^^^^^^

The main file to handle the interface between the C++ library and Unity.

.. doxygenfile:: IUnityInterface.h

IUnityProfilerCallbacks.h
^^^^^^^^^^^^^^^^^^^^^^^^^

This can be used to create custom profiling 'timestamps'

.. doxygenfile:: IUnityProfilerCallbacks.h

IUnityGraphics.h
^^^^^^^^^^^^^^^^

Gives access to the graphics API, i.e. DirectX, OpenGL, Vulkan or Metal. These each have their own specialised implementation
files. The file ``source/sample/CustomUploadMesh.cpp`` tries to use the native graphics API to access and update the GPU buffer directly.
This is related to the RenderAPI

.. doxygenfile:: IUnityGraphics.h