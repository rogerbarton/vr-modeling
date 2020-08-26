# Scene

This will elaborate on the :cs:class:`GameObject` s in the scene and how we assemble the final application from the C# scripts and prefabs.

.. note::
	Anything that is not inside the scene or referenced/invoked by it will not be in the final build.
	Most components provide tooltips, hover over an item to see what it does.


**Editable Meshes** is the parent for all meshes that will be modified by libigl as well as a spawn point to define where new meshes are positioned.

**XR Rig** contains the cameras and controllers. All components related tracking the head and hands is here. Notably the controllers have a lot of settings that you can tweak related to how the rays are shown and how you can interact with the scene. This is also where the :cs:class:`InputManager` instance is.

**UI** holds all UI items and has the :cs:class:`UiManager` instance attached.

**Environment** holds all static meshes, visual items as well as the lighting objects.

**Dll Manipulator** is and essential object which is only used in the Editor. If you are getting errors related to the using or building the dll, have a look at this object. Note that if the scene is not loaded then this script will not be running. It also runs during edit mode (when outside of play mode).