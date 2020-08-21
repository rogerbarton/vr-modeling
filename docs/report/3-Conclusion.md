# Conclusion

## Synergy of VR and 2D Editors

VR editors can be powerful for viewing and manipulating 3D meshes. However, a VR editor should work in tandem with a 2D editor. Depending on the task, it may be easier to perform it in a 2D or VR scenario. Ideally, a developer should be able to easily switch between the 2D and VR editor as desired. This is not possible with Unity without reimplementing the 2D viewer in Unity. 

Another takeaway from this project is that the iteration time when testing in VR is higher than with a 2D viewer. This is simply because a developer has to repeatedly put on and off the VR headset and controllers. If possible, testing new changes in VR are avoided. 

## Alternatives to Unity

A problem is that Unity is a game engine not a mesh editing software. Whilst it can provide a good interface to VR, there is a limit to how useful it can be, in terms of mesh editing, for libigl before common functionality has to be manually implemented, e.g. vertex selection. 

Unreal Engine is an alternative to Unity. Its benefits lie in using C++ as well as having a lower-level open source mesh API exposing greater control. However, Unreal Engine is also a game engine. A more promising alternative is Blender. There are several interesting aspects to this: 

- Open source
- Intended for editing meshes, common functionality is already available
- C++ can be used directly, otherwise the python interface from libigl can be used
- Cross-platform VR is in active development, although not production ready
- VR viewer is built on top of 2D viewer, enabling easy switching
- Blender is closer to a real world application of libigl

Both these alternatives will have a steeper learning curve, but will be better solutions in the end.

## Conclusion

A documented and working VR editor for libigl has been produced with plans for expansibility. The current 2D libigl editor still has a much larger feature set and there is a large barrier for converting applications to the VR viewer. As a result, the current application appears to be more suitable for demos. However, there is potential for this to see wider use cases in the future, if developed further. 

Using Unity as a basis is not optimal but has several advantages, particularly because of its ease of use and flatter learning curve. The discussed alternatives should be considered first before continuing this project.

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 3.