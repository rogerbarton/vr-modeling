# Implementation

The implementation has following categories:

1. Unity project setup
1. VR setup
1. Handling Input and User Interface (UI)
1. C#/C++ language interface
1. Threading of libigl calls
1. Mesh interface between libigl and Unity
1. Documentation process

## VR Interface

There are two parts to consider when adding VR functionality. The input and the output (rendering). The input part varies greatly between different VR devices and platforms, even with newer devices like the Oculus Quest having hand tracking in contrast to controllers. Initially, this project was aimed purely at the Oculus Rift S. Oculus provides an Oculus Integration on the Unity Asset Store to provide this functionality. However, since around Unity 2019.3 there has been a Unity VR Plugin system to simplify the interface with each of the SDKs of the VR platforms. Additionally, there is a Unity XR Interaction Toolkit package with provides cross-platform input as well as common VR functionality such as locomotion and interaction with UI. This package is the preferred option over the Oculus Integration.

### Locomotion

This involves moving the user in the virtual world. The common approach here is to use teleportation with a curved ray to translate the user and snap rotation to adjust the rotation of the user. Using a curved ray for indicating where to translate to has the benefit that there is an approximately linear mapping between the angle of the controller and the distance. This makes is easy to precisely indicate a teleportation position far away from the user.

## User Interface

Despite this being a VR application 2D User Interface (UI) is still necessary. It allows for displaying information as well as providing access to infrequently used actions. Frequently used actions should ideally be mapped to contextual controller input. However, if all degrees of freedom in the controller input are already used, then the 2D UI will be the fallback option.

The UI in VR is done as a world space canvas. Using a traditional screen space UI is not an option. 

### Interaction

Positioning of the UI is a essential feature. The relevant UI needs to be displayed at the right time for an intuitive experience. 

- explicit vs implicit positioning, explicit was implemented as its easier, user can decide for themselves
- Ideas of making UI collapse automatically based on attention
  - Important to remain consistency, should not be made fine grained
  - fine grained vs coarse grained
- Only active mesh UI is visible
- pop ups, problem of positioning new UI instances

Interaction with the UI relies on raycasting, as this is the most intuitive method to select UI elements.

### Generation

In order to be able to add new functionality easily, generating UI via C# scripting is necessary. The goal is to be able to easily add new UI elements and configure them, in particular setting their on click behavior. Inspired by the 2D MyDear GUI, we simply have a scrollable vertical layout group, so any child is then automatically formatted. 

For this the base UI elements were created in the editor and saved as prefabs. If advanced functionality was required a `MonoBehaviour` component was added with a simple interface. Once this preparation is done for several UI elements, once can instantiate these via script and access their components to customize them. This method has proven to be quite effective in terms of easy expansibility. 

### Performance

After performance profiling, a significant amount (>50%) of the frame time was spent raycasting the UI elements. This affected frame rates significantly leading to jitter. To reduce this a straight ray is used for the UI, as curved rays are implemented by using several straight raycasts. Additionally if the UI canvas is not being hovered over by the ray, then the UI graphics raycaster is disabled.

As the number of UI elements increases in the future, there will most likely need to be further UI performance optimizations. With an essential one being that items inside the scroll list that are not inside the mask should not be raycast or rendered. It is unclear whether this is done by default.

### Tooltips and Input Hints

To make the application more intuitive and easy to use, we need a way of providing the user with relevant help information when required. Tooltips are provided to display a short text when hovering over a UI element. Input hints tell the user what each button/axis does when pressed. Input hints are displayed directly on the controller. These can be disabled if desired, with the joystick click.

## Input

The Unity XR Interaction Toolkit is used for this for cross-platform input.

- Mapping from input to actions
- Shared vs mesh specific input

### Contextual Input

Contextual input is when we adapt what the input does based on the context or state and is important for two reasons. Firstly, a key feature of making input intuitive is by making it adapt to the current context. Secondly, contextual input helps to make maximum use of the degrees of freedom provided by the physical input device. 

Context can be inferred from the state of the application and from the input itself. In this application, two 'tools' are for manipulating objects or selections provided. These are essentially two states that we can use for inferring what the user would like to do. This can be considered as an *explicit* context as the user must explicitly set this, by choosing the tool. Ideally the context is inferred implicitly from what the user is doing. 

For example, when the user is grabbing a selection we want to provide relevant input to that context such as being able to change the pivot mode or whether rotation is enabled.

## Threading

In order to have a high framerate, the expensive computations done by libigl must be done on a worker thread. The Unity API, such as getting the transform position, is not thread-safe and thus use from a worker thread is forbidden. This has several implications, with the main one being that all access to the Unity API must be done before starting the thread and the results should be copied. This is what :cs:func:`PreExecute` and the :cs:class:`MeshState` are for. If the thread wants to make changes to the Unity state, e.g. moving an object, then this must be deferred to the main thread. Here this is done once the thread has finished in :cs:func:`PostExecute`, however, a concurrent queue of actions could also be used.

As we want to execute certain operations every frame and apply their changes, we have a loop of `PreExecute`, `Execute` and `PostExecute`. Where `PreExecute` and `PostExecute` are performed on the main thread. Notably, in `PostExecute` we apply changes to the mesh done by libigl.

C# threads are used.

Initially a model based on a concurrent queue of actions was implemented. The main thread would push an action to the queue to be performed by the worker thread. This was however not flexible enough.

## C#/C++ Interface and Data Layout

To call libigl functions a necessary C#/C++ language interface is required. This adds an extra layer of complexity. We must consider in which language functionality and data resides and what is shared. An important note is that the Unity API is only accessible within C#.

It is important to have a clear distinction of what is done where. All expensive mathematical operations are generally done in C++. The number of interface functions is kept minimal. All input is done in C# and passed as arguments.

In data we also have a distinction. Shared datatypes must be declared in both languages, including Unity types such as :cs:class:`Vector3`. The C# garbage collector also needs to be considered. Within a function this is not a problem. Data is `fixed` within the scope of a function. However, is a C# pointer is passed to C++ and this pointer is used after the function scope, then the memory may have moved and the pointer is no longer valid. This data should be pinned with :cs:func:`GcHandle.Alloc`. To avoid this scenario persistent data is allocated, and thus deleted, in C++, for example the :cpp:class:`MeshState`.

It is possible to use a C++ array in C# with the `NativeArray.ConvertExisting(ptr, Alloc.None)` and vice verse if the array is pinned and blittable (i.e. 1D)

### Compiling and CMake

CMake is used to compile the C++ library as well as the documentation in a cross-platform/IDE manner. It helps with finding libraries, but also streamlines the compile process by immediately copying the output `.dll` to the Unity project. The end product is built inside Unity. 


### Unity Plugin Reloading

Unity presents a complication that it does not unload libraries once they are loaded, which happens when it is first used. This means that we cannot recompile the C++ library without restarting Unity. This creates a larger iteration time. In order to counter this, the UnityNativeTool open source project is used. This effectively wraps native functions and un/loads the library itself. It is an editor-only tool. A few modifications were made to this in several pull requests, see [#14](https://github.com/mcpiroman/UnityNativeTool/pull/14), [#15](https://github.com/mcpiroman/UnityNativeTool/pull/15), [#18](https://github.com/mcpiroman/UnityNativeTool/pull/18), [#19](https://github.com/mcpiroman/UnityNativeTool/pull/19), [#20](https://github.com/mcpiroman/UnityNativeTool/pull/20), [#21](https://github.com/mcpiroman/UnityNativeTool/pull/21), [#28](https://github.com/mcpiroman/UnityNativeTool/pull/28) on GitHub. 

### Future Work

The C++ interface could be simplified by using a tool such as SWIG. This integrates with CMake and automatically generates the C# declarations as well as having more advanced features such as exception handling between languages. However, this simply shifts the development complexity, but it does make the language interface more robust to bugs.

Of course, alternatives to Unity such as Blender or Unreal Engine do not have this interface. In contrast, the difficult parts with the language interface have been done and development should be easier from hereon.

## Mesh Interface

Once we have modified the mesh data that is used by the renderer, such as the vertex positions, we need to apply these changes similar to `viewer.data().set_vertices(V)` in the 2D viewer. This requires access to the Unity API so must be done on the main thread. This is done in `PostExecute`. A bitmask :cs:class:`DirtyState` is used to indicate which parts have been modified and need to be updated.

An extra complication to this is that Unity used row-major and libigl expects column-major. Because of this we have two copies of the data, one in column-major and one in row-major. This creates a necessary transpose each time we apply changes, which is done in C++ on the worker thread.

### Performance

The performance of the current method seems to be good enough. Unity provides the GPU pointer to the mesh buffer. Thus a way of applying the mesh data directly to the GPU was briefly explored.

Ideally libigl would work equally well in row-major preventing a transpose and reducing the number of copies of the mesh in memory. Although Eigen supports row-major well, libigl templates do not always consider this.

## High Level Actions

### Vertex Selection

Vertex selections are used for affecting only parts of the mesh or as an input to a libigl function. A key feature is being able to transform selections with the controllers, as well as being able to transform two selections with each hand independently. This requires that we have multiple selections simultaneously. To solve this efficiently a bitmask is used. Each vertex has an additional 32-bits (represented as an integer), with each bit indicating whether it is in the selection or not. This allows for up to 32 selections, which is reasonable for this use case. 

An additional benefit of using bitmasks is that we can provide a mask of selections with one integer. For example, we can choose which selections are visible or will be translated with an integer. If we want to affect all selections we simply use the maximum integer value, where all bits are one. Functions that act on a selection have been modified, if possible, to act on a mask of selections.

When implementing this with Eigen, I noticed that it did not support bitwise operations directly. As a result, unary functions where used. These might not be as well optimized, however, the operations where fast enough on the armadillo mesh to provide responsive user interaction.

Face or edge selection was not implemented as this is more involved and does not necessarily add more features for the intended use case.

### Transformations

In order to transform a mesh or part of the mesh there are two parts. First, finding which transformation should be done and then applying it. For determining an affine transformation - translation, rotation, scaling - we are much more flexible in VR. Various different modes where tested.

Once the transformation is known we can either apply it to the mesh directly, which is done in C# with the Unity API. Or we can apply it to a vertex selection mask, which is done in C++ and modifies the vertex data. 

The implementation is a bit more involved as modifying the mesh can be done on the main thread in the normal update loop. Modifying a selection mask needs to be done on the worker thread. It uses the Eigen geometry module.

This presents a new situation when the worker thread is significantly slower than the main thread. How do we calculate the transformation? The user may grab the mesh multiple times before the transformation is applied. 

For transforming selections, different pivot modes where tested: mesh center, selection center, hand center. Where using the hand as the center appeared the most natural. Using the mesh as the center usually gave unintuitive results, especially for smaller selections.

To provide more fine grained control, the amount (from 0-1) by which the grip buttons are pressed is used as a smoothing factor. This works well, although it can be hard to control this smoothing factor precisely. It may make sense to apply a log or square root to the smoothing factor to counteract this. 

As we have two controllers, we have more freedom with how to calculate a transformation. The grip button is used to determine if we want to transform or not. When using only one hand, 

.. todo:: grabbing all selections inside brush

### Deformations

The libigl biharmonic deformation `igl::harmonic` can be toggled. If enabled it will be run whenever the input arguments have been changed. In this case, when the boundary conditions have changed. This can be detected quite easily by checking the :cpp:class:`DirtyFlags` of the mesh data have been modified when applying the mesh in :cpp:func:`ApplyDirty`.

The As-Rigid-As-Possible `igl::arap` deformation works very similarly, except that we need to check when the precomputation needs to be done.

## Documentation Process

There are several parts to the documentation process, all of which need to be equally addressed. It is important to make a distinction for how to:

1. Use the end product
1. Start development (code overview)
1. Use the existing code/API

An important part is also that the documentation should be inlined as much as possible, so it can easily be found and is made part of the source.

Most functions and types have an annotated docstring, in C# a xml-doc and javadoc in C++ so that the IDEs can display this nicely. This provides information on how to *use* the function/type. In the implementation, there are comments as required for how to *modify* the function/type. As in C# everything resides inside a class/struct/interface the docstring of the class is intended to give an overview of everything inside and its intention.

Additional markdown files are there to add an overview of the files and provide general information not specific to a file or piece of code. These files are placed 'inline' next to the `.cs` or `.cpp` files. 

To condense all this information, Doxygen and Sphinx are used. Doxygen is used to extract the documentation from the code. This information in xml format is then used by Breathe (a Sphinx extension) to render it with Sphinx, which then combines it with the markdown files. Breathe and the language domains ensure cross-referencing of items. 

For this to work with C#, the *sphinx-csharp* and *breathe* projects where modified, see [#8](https://github.com/djungelorm/sphinx-csharp/pull/8) and [#550](https://github.com/michaeljones/breathe/pull/550) respectively on GitHub.

ReadTheDocs is used to host and compile the website output of Sphinx. This has continuous integration. Whenever a commit is pushed to the `read-the-docs` branch, the website is recompiled.

## Miscellaneous Features

1. Importing of meshes into Unity, adjusting vertex buffer layout and materials, scaling
   1. Recognized file types use an asset post-processor
   1. Custom file types, e.g. `.off`, are imported via libigl within a scripted importer
   1. Validation of meshes before instantiation
1. UI to indicate when thread is inside `Execute` for a longer time
1. Dither shader on active mesh and controllers when occluded using URP custom render passes
1. Environment modelled in Blender, ocean shader done in Shader Graph
1. Speech recognition for specific actions (disabled by default)
1. 