# Introduction

.. note::

   This report will focus on the development approach as well as evaluating the project and suggesting future improvements. *For implementation details and how to use this project see the online documentation at https://vr-modeling.readthedocs.io/.*

## Purpose

The purpose of this thesis is to produce an extensible virtual reality (VR) viewer and editor for use with the libigl library :cite:`libigl`. In effect converting the current 2D user interface to the VR setting. Potential use cases include visualizations of 3D models and operations on them, such as those provided by libigl. 

.. :: The thesis also explores new ways of interacting in VR.

VR provides an alternative input and output format in comparison to a conventional 2D Screen with a keyboard and mouse. It allows for accurate representation of 3D scenes, notably in terms of depth and scale, due to its stereoscopic rendering.

In terms of input, VR controllers can give precise 3D positional and rotational input for each hand in comparison to 2D positional input from a mouse. This is also superior to 3D mice, which only offer relative 3D positional and rotational input. VR is useful in our scenario as it allows for easier and more intuitive interaction with a mesh. 

.. :: For certain applications VR will provide significant benefits. 3D modeling presents itself as one of these applications.

## Related Work

Several 3D modeling applications already exist on the Oculus Store such as [Google Blocks](https://arvr.google.com/blocks/), [Facebook's Quill](https://quill.fb.com/). These offer construction of 3D scenes from primitives and brush strokes. Quill also offers being able to animate these. [Oculus Medium](https://www.oculus.com/medium/) provides more advanced functionality with sculpting. [Blender](https://www.blender.org/) has recently added a [VR scene inspection](https://docs.blender.org/manual/en/dev/addons/3d_view/vr_scene_inspection.html) ([release notes](https://wiki.blender.org/wiki/Reference/Release_Notes/2.83/Virtual_Reality)) add-on. This enables the user to view a scene in VR. [Unreal Engine](https://www.unrealengine.com/) has also developed a [VR mode](https://docs.unrealengine.com/en-US/Engine/Editor/VR/index.html) which can be used for level design. 

In summary, there already exist several VR editors for 3D modeling and animation. However, this is still a developing field with little standardization. The deformation of meshes in VR has not yet been explored, particularly with libigl, which is what this thesis focuses on.

## Development Approach

The Oculus Rift S headset was used as the primary target device. 

For implementing this the [Unity](https://unity.com/) game engine was chosen, partly due to the experience with the engine. This provides many standard features as well as a cross-platform VR integration. It offers advanced VR features such as [single-pass stereo rendering](https://docs.unity3d.com/Manual/SinglePassStereoRendering.html), which provides great performance benefits. It has an easy way of adding functionality via C# scripts. This, however, creates a necessary language interface to C++ such that libigl can be used. 

Using the Oculus SDK directly requires too much development overhead and will have less features as a result. It will also be significantly harder to maintain. Using a game engine which already provides a plethora of features is the best option.

For example, use cases of libigl two mesh deformations where chosen, a biharmonic deformation and an As-Rigid-As-Possible deformation. These each require selection of parts of the mesh as well as ways of transforming these, such that we can provide boundary conditions for the libigl algorithms. The deformations have been chosen from the libigl tutorial. The intent is that further libigl functionality, of any kind, can also be added. 

Ideally, existing libigl applications would be able to simply switch which viewer is being used, between the 2D GLFW viewer and the VR viewer. This is not possible with this development approach. This is because libigl will be a library used by the VR editor and not the converse, due to how Unity executables are built. As a result, the interface to the VR viewer cannot be the same as the 2D GLFW viewer.

A workaround to this would be to implement inter-process communication between a libigl executable and a Unity built executable. This is however more involved and outside of the scope of this thesis. It is also unclear whether this approach will yield performant and maintainable results. 