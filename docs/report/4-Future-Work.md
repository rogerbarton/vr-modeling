# Results & Future Work

Extension with [Discrete orthogonal geodesic nets (DOG)](https://github.com/MichaelRabinovich/DOG-editor/) is another use case. This was briefly explored. However, the interface is more complicated and the project is currently not compatible with windows. 

A problem is that Unity is a game engine not a mesh editing software. A promising alternative to the Unity game engine as a base is Blender. There are several interesting aspects to this: 

- open source
- intended for editing meshes, common functionality is already available
- cross-platform VR is in active development, although not production ready
- C++ can be used directly, otherwise the python interface from libigl can be used
- blender is closer to a real world application of libigl

Unreal engine is another alternative

Both these alternatives will have a steeper learning curve but will be better solutions in the end.