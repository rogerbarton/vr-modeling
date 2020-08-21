# Conclusion

## Future Work

[Discrete orthogonal geodesic nets (DOG)](https://github.com/MichaelRabinovich/DOG-editor/) :cite:`dog-editor` as another use case were briefly explored. However, the interface is more complicated and the project is currently not compatible with windows without modifications to the code. 

### Alternatives to Unity

A problem is that Unity is a game engine not a mesh editing software. A promising alternative to the Unity engine as a base is Blender. There are several interesting aspects to this: 

- Open source
- Intended for editing meshes, common functionality is already available
- Cross-platform VR is in active development, although not production ready
- C++ can be used directly, otherwise the python interface from libigl can be used
- Blender is closer to a real world application of libigl

Unreal Engine is another alternative. Its benefits lie in using C++ as well as having a lower-level open source mesh API exposing greater control. However, Unreal Engine is also a game engine.

Both these alternatives will have a steeper learning curve but will be better solutions in the end.

Another takeaway from this project is that the iteration time when testing in VR is higher than with a 2D viewer. This is simply because a developer has to repeatedly put on and off the VR headset. If possible testing new changes in VR was avoided. 

Depending on the task it may be easier to perform it in a 2D or VR scenario. Ideally a developer should be able to easily switch between the 2D and VR editor as desired. This is not possible with Unity without reimplementing the 2D viewer in Unity. For this the Blender VR scene inspection appears to be much more suitable. 

## Conclusion

VR editors can be powerful for viewing and manipulating 3D meshes. However, a VR editor should work in tandem with a 2D editor. The current application appears to be more suitable for demos. 

Using Unity as a basis is not optimal but has several advantages. The discussed alternatives should be considered first before continuing this project.


.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt