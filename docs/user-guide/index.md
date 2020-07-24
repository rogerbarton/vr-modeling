# User Guide

.. tip::

	Most things you can do are indicated by the controller hints, these adapt based on the tool being used. Hover over a UI element to display the tooltip on your left hand.

The rays/lasers are only for UI and teleportation currently.
You can grab UI panels and move them around by using the grip when pointing at them.

There is a tool/mode system with currently two modes:

- **Selection** *i.e. Blender edit mode*
  - Edit and grab (multiple) vertex selections
- **Transform** Mesh *i.e. Blender object mode*
  - Move the whole mesh around and select which one can be edited

There are several UI Panels, a generic one and one per mesh. The UI of this mesh is highlighted. 

The active mesh is the one currently being modified and will be highlighted if occluded. The active mesh can be set in the top right of the UI of the mesh or in the Transform tool with the brush. 

From the UI panel you can see information about the mesh as well as performing more advanced operations. We can enable a deformation from here, currently the libigl biharmonic and as-rigid-as-possible deformations. Deformations can be executed once or continuously every frame.

.. toctree::
   :maxdepth: 2
   :caption: Contents:
   
   gallery

