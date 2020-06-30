# Adding Functionality

## Threading PrePostExecute Cycle

.. note::

	The Unity API is not thread safe, only simple methods like Debug.Log can be used in a thread.

<iframe frameborder="0" style="width:100%;height:200px;" src="https://app.diagrams.net/?lightbox=1&highlight=0000ff&nav=1&title=PrePostExecute#Uhttps%3A%2F%2Fdrive.google.com%2Fuc%3Fid%3D13g6p1HSJ_EPnPZU7FR49HOlWYZDNAAtS%26export%3Ddownload"></iframe>

In order to keep the virtual reality experience responsive and at high framerates, all the geometry and libigl calls are on a worker thread. Each mesh has its own thread. As the Unity API is not thread safe, it can only be accessed from the main thread. API calls must be made in PreExecute and their results copied to the thread via the `MeshInputState`. Because of this we have the cycle shown above.

- `PreExecute` - this is where any preparation is done that needs to be on the main thread. The shared `InputState` is copied. The `MeshInputState Input` is copied to the thread version `ExecuteInput`
- `Execute` - depending on the `LibiglBehaviour.ExecuteInput` when starting the thread we call different code, e.g. if `MeshInputState.DoSelect` is true we modify the selection
- `PostExecute` - this is where changes are applied to the Unity mesh.
- `Update` - this is the *separate* Unity callback called every frame, in this we update the `LibiglBehaviour.Input` state.

## Data Storage

There is a lot of data tied to the mesh and the user input. It is important to know where what is stored.

Generally, data falls into one of the following categories:

1. **Input data**
   1. Data that is used as to parametrize and decide what is executed (usually from UI/Input), this belongs to the C# only `MeshInputState` or the `InputState` if it is shared between meshes.
1. **Mesh data**
   1. Vertex/Face data required for rendering the mesh, this is the most complicated. It must be part of the :cpp:struct:`MeshState` and shared between C# and C++. There is a lifecycle to this detailed in :ref:`Applying Mesh Data`.
   1. Data that is used only for computations, this belongs to the C++ only :cpp:struct:`MeshStateNative`.
   1. (uncommon) data that must be shared between C++ and C#, such as results of a computation (e.g. selection size), this also belongs to the :cpp:struct:`MeshState` 

## Custom Deformation

To add a new deformation there are several things that need to be done. The approach I often use is to start with the 
complicated C++, then the C# interface and end with the UI/input:

1. How the deformation is carried out in the C++, see `Deform.cpp`
    1. Be sure to set which data from the mesh you have changed with the `State->DirtyState` variable.
1. Storing your data in the right place, see `MeshState*.h`
1. Making this callable from C#: declare in `Interface.h` and redeclare in `Native.cs`
1. Integrating with the Pre/Post/Execute threading loop, see `LibiglBehaviour*.cs` and `MeshInput.cs`
    1. How this deformation is executed from C#, see `Libigl/LibiglBehaviour.Actions.cs`
    1. Handling of the controller input to determine when to execute the deformation, see `LibiglBehaviour.Input.cs`
1. Parametrization in the 2D UI, see examples of UI generation in `UI/UiDetails.cs`

This is indeed quite a long list because of the following complications:

1. C#/C++ interface
1. Threading in Unity, not being able to access any of the Unity API a worker thread.

## Applying Mesh Data

To apply changes to the vertex matix :math:`V`, or any of the other matrices in the :cpp:struct:`MeshState` you need to set the :cpp:member:`MeshState::DirtyState` with the appropriate :cpp:enum:`DirtyFlag`. This tells the system what has changed and the rest will be done automatically. For more control you might want to see `IO.cpp` :cpp:func:`ApplyDirty`. 

This is only for data that has to be made available to Unity to render the mesh.

### Mesh Data Lifecycle (Advanced)

This details how changes to the mesh are propagated to Unity and its renderer. The example is done with the vertex matrix :math:`V` but works also for the other data in :cpp:struct:`MeshState`. 

.. note::

	Unity stores its mesh data in **Row Major**, whereas libigl requires **Column Major**, a necessary conversion by transposing has to be made.


<iframe frameborder="0" style="width:100%;height:220px;" src="https://app.diagrams.net/?lightbox=1&highlight=0000ff&nav=1&title=ApplyMeshData#Uhttps%3A%2F%2Fdrive.google.com%2Fuc%3Fid%3D1vsv6ZD3W_HRIGBaCqMOHjp-v1YPSuARU%26export%3Ddownload"></iframe>

1. *(in Execute)* The developer modifes the V matrix and sets it as dirty: :cpp:expr:`state->DirtyState |= DirtyFlag.VDirty`
1. *(in PostExecute)* :cpp:func:`ApplyDirty` is called to apply the changes from the :cpp:struct:`MeshState` to the Unity row major copy pointed to in :cpp:struct:`UMeshDataNative`. Here we also filter out only things that have changed. This is called by `UMeshData.cs`.
1. Once this transposing is done, we pass the data to Unity in `UMeshData::ApplyDirtyToMesh` in C#

## Custom Shader

This is quite easy with the new Unity **Shader Graph**. So no HLSL/GLSL is required for most things. Have a look at the default shader `Materials/Shaders/VertexColor` being used on the meshes. Note that to display the shader in Unity you must put the shader into a material and then attach that to the mesh renderer component on the GameObject. See `Materials/EditableMesh.mat` which uses the `VertexColor` shader. 

Vertex data, e.g. vertex position or uv coordinates, can be accessed via a node in the graph.