# Discussion & Future Work

[Discrete orthogonal geodesic nets (DOG)](https://github.com/MichaelRabinovich/DOG-editor/) :cite:`dog-editor` as another use case were briefly explored. However, the interface is more complicated and the project is currently not compatible with windows without modifications to the code. 

Whilst developing, motion sickness experienced in VR has been minimal as the user is generally experiencing little movement. However in order to decrease motion sickness for newer users in the future, the field of view can be reduced when moving with a vignette effect :cite:`vr-tunneling-paper`. This already has a Unity implementation with [VR Tunneling Pro](https://assetstore.unity.com/packages/tools/camera/vr-tunnelling-pro-106782) :cite:`vr-tunneling-pro-asset`.

## Synergy of VR and 2D Editors

VR editors can be powerful for viewing and manipulating 3D meshes. However, a VR editor should work in tandem with a 2D editor. Depending on the task, it may be easier to perform it in a 2D or VR scenario. Ideally, a developer should be able to easily switch between the 2D and VR editor as desired. This is not possible with Unity without reimplementing the 2D viewer in Unity. 

Another takeaway from this project is that the iteration time when testing in VR is higher than with a 2D viewer. This is simply because a developer has to repeatedly put on and off the VR headset and controllers. If possible, testing new changes in VR are avoided. 

## Alternatives to Unity

A problem is that Unity is a game engine not a mesh editing software. Whilst it can provide a good interface to VR, there is a limit to how useful it can be, in terms of mesh editing, for libigl before common functionality has to be manually implemented, e.g. vertex selection. Finding workarounds for Unity incompatibilities was, in the end, a large focus of this thesis, see Backend.

Unreal Engine is an alternative to Unity. Its benefits lie in using C++ as well as having a lower-level open source mesh API exposing greater control. However, Unreal Engine is also a game engine. A more promising alternative is Blender. There are several interesting aspects to this: 

- Open source
- Intended for editing meshes, common functionality is already available
- C++ can be used directly, otherwise the python interface from libigl can be used
- Cross-platform VR is in active development, although not production ready
- VR viewer is built on top of 2D viewer, enabling easy switching
- Blender is closer to a real world application of libigl

Both these alternatives will have a steeper learning curve, but will be better solutions in the end.

## Use Cases

The main use case is for demonstrations of new geometry algorithms in VR. Integrating such a new algorithm would still require a reasonable amount of work, see [developer guide](https://vr-modeling.readthedocs.io/docs/developer-guide/index.html). In fact, most of the use cases originate from the individual components. 

Runtime mesh modifications can be done more easily, as the mesh interface between C++ Eigen matrices and Unity is now working. This is regardless whether in VR, with libigl or not. Developments made with the UnityNativeTool allow for easier C++ development within Unity for any project. This project can also serve as an example for C#/C++ development in Unity as this is not common in open source. The methods used for UI can be used in other VR projects. Other projects using C# and C++ can use the same documentation generation process.

## Shortcomings

The large weakness of this project has already been discussed. This lies in the choice of Unity as a basis, which ultimately limits its future potential. Otherwise, in comparison to the existing applications it offers very little functionality, with only two types of operations to perform on a mesh. There is also no way of exporting the meshes. Although, this could be done in the future with the existing FBX, USD and Alembic exporters in Unity. 

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 4.