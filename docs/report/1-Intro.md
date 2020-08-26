# Introduction

.. note::

   This report will focus on the development approach as well as evaluating the project and suggesting future improvements. *For implementation details and how to use this project see the online documentation, where the gallery can also be found* :cite:`docs`.

## Purpose

The purpose of this thesis is to produce an extensible virtual reality (VR) viewer and editor for use with the [libigl library](https://libigl.github.io/) :cite:`libigl`. In effect converting the current 2D user interface to the VR setting. Potential use cases include visualizations of 3D models and operations on them, such as those provided by libigl. 

VR provides an alternative input and output format in comparison to a conventional 2D screen with a keyboard and mouse. It allows for accurate representation of 3D scenes, notably in terms of depth and scale, due to its stereoscopic rendering.

In terms of input, VR controllers can give precise 3D positional and rotational input for each hand in comparison to 2D positional input from a mouse. This is also superior to 3D mice, which only offer relative 3D positional and rotational input. VR is useful in our scenario as it allows for easier and more intuitive interaction with a mesh. 

.. :: For certain applications VR will provide significant benefits. 3D modeling presents itself as one of these applications.

## Related Work

Advances in VR input methods, with 6DOF controllers for each hand, has allowed for more innovation in 3D modeling. Several 3D modeling applications already exist on the Oculus Store such as [Google Blocks](https://arvr.google.com/blocks/) :cite:`google-blocks` and [Facebook's Quill](https://quill.fb.com/) :cite:`quill`. These offer construction of 3D scenes from primitives and brush strokes. Both use a 'palette'-like user interface (UI) on the secondary hand for adding and manipulating meshes. Blocks has more basic functionality, but integrates with a sharing service online. Quill also offers snapping tools and being able to animate scenes. 

[Oculus Medium](https://www.oculus.com/medium/) :cite:`oculus-medium` provides more advanced functionality with sculpting and uses an unconventional method for storing its meshes allowing for fast boolean operations. "Medium defines 3D objects (sculpts) using an implicit surface. The surface is stored as a signed distance field (SDF) in a 3D grid of voxels." :cite:`oculus-medium-dev-blog`.

[Blender](https://www.blender.org/) :cite:`blender` has recently added a [VR scene inspection](https://docs.blender.org/manual/en/dev/addons/3d_view/vr_scene_inspection.html) :cite:`blender-vr` add-on, see also [release notes](https://wiki.blender.org/wiki/Reference/Release_Notes/2.83/Virtual_Reality) :cite:`blender-vr-release-notes`. This currently only enables the user to view a scene in VR and does not make use of the controllers. [Unreal Engine](https://www.unrealengine.com/) :cite:`unreal-engine` since version 4.17 has developed a [VR mode](https://docs.unrealengine.com/en-US/Engine/Editor/VR/index.html) :cite:`unreal-engine-vr-mode` for its editor. This is primarily used for level design and previewing scenes. It replicates its 2D editor windows in VR as floating panels allowing a similar level of functionality to the 2D editor. It also makes use of nested pie menus for common actions like snapping. Both Blender and Unreal Engine allow for easy switching between 2D and VR. 

In summary, there already exist several VR editors for 3D modeling and animation. However, this is still a developing field with little standardization. User interface is generally interacted with by using raycasting and a secondary hand is often dedicated for quick UI actions. Except for sculpting, complex deformation of meshes in VR has not yet been explored. This is what this thesis focuses on.

## System Components

The Oculus Rift S headset was used as the primary target device. 

For implementing this the [Unity](https://unity.com/) :cite:`unity` game engine was chosen, partly due to experience with the engine. It provides many standard features as well as a cross-platform VR integration. It offers advanced VR features such as [single-pass stereo rendering](https://docs.unity3d.com/Manual/SinglePassStereoRendering.html), which provides great performance benefits. Furthermore, it has an easy way of adding functionality via C# scripts. This, however, creates a necessary language interface to C++ such that libigl can be used. 

Using the Oculus SDK directly requires too much development overhead and will result in less features. It will also be significantly harder to maintain. Using a game engine which already provides a range of features is the best option given the time available.

For example use cases of libigl two mesh deformations where chosen, a biharmonic deformation :cite:`harmonic-paper` and an As-Rigid-As-Possible deformation :cite:`arap-paper`. These each require selection of parts of the mesh as well as ways of transforming these, such that we can provide boundary conditions for the libigl algorithms. The deformations have been chosen from the libigl tutorial. The intent is that further libigl functionality, of any kind, can also be added. 

Ideally, existing libigl applications would be able to simply switch which viewer is being used, either the current libigl 2D GLFW viewer and the VR viewer. This is not possible with this development approach. This is because libigl will be a library used by the VR editor and not the converse, due to how Unity executables are built. As a result, the interface to the VR viewer cannot be the same as the 2D GLFW viewer.

A workaround to this would be to implement inter-process communication between a libigl executable and a Unity built executable. This is however more involved and outside of the scope of this thesis. It is also unclear whether this approach will yield performant and maintainable results. 

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 1.