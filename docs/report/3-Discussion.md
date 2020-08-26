# Discussion & Future Work

[Discrete orthogonal geodesic nets (DOG)](https://github.com/MichaelRabinovich/DOG-editor/) :cite:`dog-editor` as another use case were briefly explored. However, the interface is more complicated and the project is currently not compatible with Windows without modifications to the code. 

Motion sickness experienced in VR has been minimal as the developer is generally experiencing little movement. However, motion sickness for newer users could be further improved if needed. This can be done by reducing the field of view during fast movements with a vignette effect :cite:`vr-tunneling-paper`. This already has a Unity implementation with the [VR Tunneling Pro asset](https://assetstore.unity.com/packages/tools/camera/vr-tunnelling-pro-106782) :cite:`vr-tunneling-pro-asset`, so will require minimal work to add.

## Synergy of VR and 2D Editors

VR editors can be powerful for viewing and manipulating 3D meshes. However, a VR editor should work in tandem with a 2D editor. Depending on the task, it may be easier to perform it in a 2D or VR scenario. Ideally, a developer should be able to easily switch between the 2D and VR editor as desired. This is not possible with Unity without reimplementing the 2D viewer in Unity. 

Another takeaway from this project is that the iteration time when testing in VR is higher than with a 2D viewer. This is simply because a developer has to physically put on and off the VR headset and controllers repeatedly. If possible, testing new changes in VR is avoided. 

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

## Use Cases & Shortcomings

This project can be used to visualize libigl functionality in VR. However, there is still a reasonable amount of work required to integrate new behavior. In most cases, this will not be worth the effort.

The most immediate value is in the independent components. Runtime mesh modifications can be done more easily in Unity, as the mesh interface between C++ Eigen matrices and Unity is now working. This is regardless whether in VR, with libigl or not. Developments made with the UnityNativeTool allow for easier C++ development within Unity for any project. This project can also serve as an example for C#/C++ development in Unity, as this is not common in open source. The methods used for UI can be used in other VR projects. Other projects using C# and C++ can use the same documentation generation process.

The major shortcoming of this project has already been introduced and lies in the choice of Unity as a basis. This is ultimately limits its future potential. Otherwise in comparison to the related work, it offers very little functionality, with only two types of operations to perform on a mesh. There is currently no way of exporting the meshes, although, this could be done in the future with the existing FBX, USD and Alembic exporters in Unity. 

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 4.