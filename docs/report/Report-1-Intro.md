# Introduction

The goal of this thesis is to produce an extensible virtual reality (VR) viewer and editor for use with the libigl library. 

## Why VR?

VR is useful in this scenario as it allows for easier interaction with the mesh, as we have completely different input methods with two controllers instead of a conventional keyboard and mouse. Notably these controllers can give precise 3D positional and rotational input for each hand in coparison to 2D positional input from a mouse. This is also superior to 2D mice, which only offer relative 3D positional and rotational input. Perception of 3D objects, in terms of depth and scale, is more natural than a 2D screen. 

## Implementation Approach

For implementing this the Unity engine was chosen. This provides many features as well as a good cross-platform VR integration. It also offers advanced VR features such as single-pass instanced rendering, which offers great performance benefits. It has an easy way of adding functionality via C# scripts and newly visual scripting with Bolt. This, however, creates a necessary language interface to C++ such that libigl can be used. Using the libigl python bindings is not a viable option with Unity and would also require a language interface.

For example use cases of libigl two mesh deformations where chosen, a biharmonic deformation and an As-Rigid-As-Possible deformation. These each require selection of parts of the mesh as well as ways of transforming these, so that we can provide boundary conditions for the libigl algorithms. The deformations have been chosen from the libigl tutorial. The intent is that further libigl functionality, of any kind, can also be used with this. 

