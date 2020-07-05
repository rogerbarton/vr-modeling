# Welcome to vr-modeling's documentation! <a href='https://vr-modeling.readthedocs.io/?badge=latest'><img src='https://readthedocs.org/projects/vr-modeling/badge/?version=latest' alt='Documentation Status' /></a>

*Deform meshes in Virtual Reality with libigl using Unity.*

.. toctree::
   :maxdepth: 2
   :caption: Contents:
   :numbered:

   self
   docs/user-guide/index
   docs/developer-guide/index
   Assets/index
   Interface/index

- See whole :ref:`genindex`

## Features

.. TODO: Add gifs

1. Run As-Rigid-As-Possible `igl::arap` or a biharmonic defomation `igl::harmonic` on a mesh and manipulate it
   in real-time
1. Select vertices and transform them using VR controllers
1. Threaded geometry code, can handle armadillo with responsive VR
1. Multi-mesh editing possible
1. Multiple selections per mesh (using bitmasks)
1. Easy import process of new models
1. Easy UI generation

**Compatible VR Headsets:** Theoretically everything compatible with the Unity [XR Plugin](https://docs.unity3d.com/Manual/XR.html)
system and [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html).
Tested on Oculus Rift S, likely to be compatible are Oculus Rift, Oculus Quest, Valve Index.

## Technical Features

1. Unity C#/C++ interface
1. Unity/libigl interface for meshes
1. Handling of input with threaded geometry calls

## Development Timeline

<iframe allowfullscreen src='https://timelines.gitkraken.com/timeline/c1c573c02b5749eca69a3107f3b57999?showControlPanel=true&showMinimap=true&allowPresentationMode=true' style='width:100%;height:500px;border:none;'></iframe>
