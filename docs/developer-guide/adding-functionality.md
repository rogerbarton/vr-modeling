# Adding Functionality

To add a new deformation there are several things that need to be done. The approach I often use is to start with the 
complicated C++, then the C# interface and end with the UI/input:

1. How the deformation is carried out in the C++, see `Deform.cpp`
1. Storing your data in the right place, see `MeshState*.h`
1. Making this callable from C#: declare in `Interface.h` and redeclare in `Native.cs`
1. Integrating with the Pre/Post/Execute threading loop, see `LibiglBehaviour*.cs` and `MeshInput.cs`
    1. How this deformation is executed from C#, see `Libigl/LibiglBehaviour.Actions.cs`
    1. Handling of the controller input to determine when to execute the deformation, see `LibiglBehaviour.Input.cs`
1. Parametrization in the 2D UI, see examples of UI generation in `UI/UiDetails.cs`

This is indeed quite a long list because of the following complications:

1. C#/C++ interface
2. Threading in Unity, not being able to access any of the Unity API a worker thread.