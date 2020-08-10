# Introduction

## Purpose

The purpose of this thesis is to produce an extensible virtual reality (VR) viewer and editor for use with the libigl library. Potential use cases include visualizations of 3D models and operations on them, such as those provided by libigl.

## Why VR?

VR provides an alternative input and output in comparison to a 2D Screen with a keyboard and mouse.

VR allows for accurate representation of 3D scenes, notably in terms of depth and scale. 

VR is useful in this scenario as it allows for easier interaction with the mesh, as we have completely different input methods, with two controllers instead of a conventional keyboard and mouse. Notably these controllers can give precise 3D positional and rotational input for each hand in comparison to 2D positional input from a mouse. This is also superior to 3D mice, which only offer relative 3D positional and rotational input.

For not all but certain applications VR will provide significant benefits. 3D modeling presents itself as one of these applications.

## Development Approach

The Oculus Rift S headset was used as the primary target device. 

For implementing this the Unity engine was chosen. This provides many features as well as a good cross-platform VR integration. It has an easy way of adding functionality via C# scripts and newly visual scripting with Bolt. This, however, creates a necessary language interface to C++ such that libigl can be used. Using the libigl python bindings is not a viable option with Unity and would also require a language interface. It also offers advanced VR features such as [single-pass stereo rendering](https://docs.unity3d.com/Manual/SinglePassStereoRendering.html), which offers great performance benefits.

For example use cases of libigl two mesh deformations where chosen, a biharmonic deformation and an As-Rigid-As-Possible deformation. These each require selection of parts of the mesh as well as ways of transforming these, so that we can provide boundary conditions for the libigl algorithms. The deformations have been chosen from the libigl tutorial. The intent is that further libigl functionality, of any kind, can also be used with this. 

