# User Guide

.. tip::

	Most things you can do are indicated by the controller hints, these adapt based on the tool being used. Hover over a UI element to display the tooltip on your left hand.

The rays/lasers are only for UI and teleportation currently.
You can grab UI panels and move them around.

Current tools/modes:
- **Selection**
  - Edit and grab (multiple) selections
- **Transform** Mesh
  - Move the whole mesh around and select which one can be edited.

*For users familiar with blender this is effectively Edit and Object mode.*

There are several UI Panels, a generic one and one per mesh. There is a notion of an active mesh, the one currently being edited. The UI of this mesh is highlighted. The active mesh can be selected in the UI or Transform tool with the brush and will be highlighted if occluded. 

From the UI panel you can see information about the mesh as well as performing more advanced operations. We can enable a deformation from here, currently the libigl biharmonic and as-rigid-as-possible deformations.

.. toctree::
   :maxdepth: 2
   :caption: Contents:
   
   gallery

