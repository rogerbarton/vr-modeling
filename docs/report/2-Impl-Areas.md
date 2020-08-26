# Method

The implementation has following categories:

1. Backend
   1. C#/C++ language interface
   1. Threading of libigl calls
   1. Mesh interface between libigl and Unity
1. Frontend
   1. VR interface
   1. Handling of input and user interface (UI)
   1. High-level actions utilizing libigl
1. Documentation process

## Backend

### Threading

In order to have a high framerate, the expensive computations done by libigl must be  performed on a worker thread. The Unity API, such as getting the transform position, is not thread-safe and thus use from a worker thread is forbidden. This has several implications, with the main one being that all access to the Unity API must be done before starting the thread and the results should be copied. This is what :cs:func:`PreExecute` and the :cs:struct:`MeshState` are for. 

If the thread wants to make changes to the Unity state, e.g. moving an object, then this must be deferred to the main thread. Here this is done once the thread has finished in :cs:func:`PostExecute`, however, a concurrent queue of actions could also be used.

As we want to execute certain operations every frame and apply their changes, we have a loop of `PreExecute`, `Execute` and `PostExecute`. Where `PreExecute` and `PostExecute` are performed on the main thread. Notably, in `PostExecute` we apply changes to the mesh done by libigl.

Initially, the Unity C# Job System was used. However, this did not provide enough control as only value types could be passed to the thread in order to prevent race conditions. Another model based on a concurrent queue of actions was implemented. The main thread would push an action to the queue to be performed by the worker thread. This was not flexible enough and there was a lack of clear control over the ordering of actions.

### C#/C++ Language Interface

To call libigl functions a necessary C#/C++ language interface is required. This adds an extra layer of complexity. We must consider in which language functionality and data resides and what is shared. An important note is that the Unity API is only accessible within C#. Using the libigl python bindings would also require a language interface as Unity does not directly support python.

#### Functionality

It is important to have a clear distinction of what is done where. All expensive mathematical operations are generally done in C++. The number of interface functions is kept minimal. All input collection and high-level actions are done in C#.

Development of a C++ library inside Unity is particularly challenging as an unhandled exception or runtime error will crash the Unity editor. This can be mitigated be placing assertions, which pause the execution and allow for the attachment of a debugger.

#### Data

In data we also have a distinction. Shared datatypes are possible but must be declared in both languages, including Unity types such as :cs:class:`Vector3`. By convention, each mesh has a state shared between the languages. This will point also to any other native-only data. By doing this, we can easily handle multiple meshes and in future serialization.

When the pointers are used the C# garbage collector needs to be considered. Within a function this is not a problem as data is `fixed` throughout a native function call. However, if a C# data is passed to C++ and its pointer is used after the function scope, then the memory may have moved and the pointer is no longer valid. This data should be pinned with :cs:func:`GcHandle.Alloc`. To avoid this scenario persistent data is allocated, and thus deleted, in C++, notably the :cpp:struct:`MeshState`.

Marshalling of managed types are an additional consideration. Passing large non-blittable types to a native function can result in expensive memory operations, in particular 2D C# arrays which have a different layout in each language. 

#### Compiling and CMake

CMake is used to compile the C++ library as well as the documentation in a cross-platform/IDE manner. It helps with finding libraries, but also streamlines the compile process by immediately copying the output `.dll` to the Unity project. The end product is built inside Unity. This also ensures the project remains cross-platform.

#### Unity Plugin Reloading

Unity presents a complication by never unloading libraries once they are loaded, which happens lazily when they are first used. This means that we cannot recompile the C++ library without restarting Unity, creating a much larger iteration time. In order to counter this, the [UnityNativeTool](https://github.com/mcpiroman/UnityNativeTool) :cite:`unity-native-tool` open source project is used. This effectively wraps native functions and un/loads the library itself. It is an editor-only tool. A few modifications were made to this in several pull requests, see [#14](https://github.com/mcpiroman/UnityNativeTool/pull/14), [#15](https://github.com/mcpiroman/UnityNativeTool/pull/15), [#18](https://github.com/mcpiroman/UnityNativeTool/pull/18), [#19](https://github.com/mcpiroman/UnityNativeTool/pull/19), [#20](https://github.com/mcpiroman/UnityNativeTool/pull/20), [#21](https://github.com/mcpiroman/UnityNativeTool/pull/21), [#28](https://github.com/mcpiroman/UnityNativeTool/pull/28) on GitHub. 

#### Future Work

The C++ interface could be simplified by using a tool such as [SWIG](http://www.swig.org/) :cite:`swig`, see also :cite:`swig-paper`. This integrates with CMake and automatically generates the C# declarations as well as having more advanced features such as exception handling between languages. Primarily, it removes redundant code and documentation. However, this simply shifts the development complexity, but it does make the language interface more robust to bugs.

Of course, alternatives to Unity such as Blender or Unreal Engine do not have this interface. In contrast, the difficult parts with the language interface have been done and development should be easier from hereon.

Additionally, implementing serialization of the entire C++ state with the help of `igl::serialize` would be beneficial. This state would only involve all :cpp:struct:`MeshState` instances. It would enable faster testing as well as enabling the use hot-reloading of the C++ library while paused, decreasing the iteration time. Hot-reloading was attempted by simply not deleting the allocated :cpp:struct:`MeshState` memory and retaining the pointers in C#. However as the library is fully unloaded, its entire memory is deallocated and the C# pointers are invalidated.

### Mesh Interface

Once we have modified the mesh data used by the renderer, such as the vertex positions, we need to apply these changes. This is the equivalent of `viewer.data().set_vertices(V)` in the 2D viewer. This requires access to the Unity API, so must be done on the main thread. It is done in :cs:func:`PostExecute`. A bitmask :cs:var:`DirtyState` is used to indicate which parts have been modified and need to be updated. This is done in a coarse-grained fashion. For example, if a single vertex is moved the entire position matrix is updated. This sparse editing of the mesh occurs frequently, for example when an operation is performed on a selection. As a result, this could be a potential area of improvement, which could be fixed by accessing the GPU buffer directly, see `More on Performance`_.

An extra complication to this is that Unity uses row-major and libigl expects column-major matrices. Because of this we have two copies of the data, one in column-major and one in row-major. This creates a necessary transpose each time we apply changes. To mitigate performance losses, this is done in C++ on the worker thread. For larger meshes the effect of this transpose on runtime as well as memory performance will be more noticeable. For the meshes tested, this was not an issue with operations on the [armadillo mesh](http://graphics.stanford.edu/data/3Dscanrep/) :cite:`standford-3d-models` still being responsive. 

Ideally libigl would work equally well in row-major preventing a transpose and reducing the number of copies of the mesh in memory. Although Eigen :cite:`eigen` supports row-major well, libigl templates do not always consider this causing compiler errors.

In this part of the development process, the engine source code would have been beneficial. 

#### More on Performance

Unity provides the GPU pointer to the mesh buffer. Thus a way of applying the mesh data directly to the GPU was briefly explored with help of the [NativeRenderingPlugin](https://bitbucket.org/Unity-Technologies/graphicsdemos) :cite:`unity-native-rendering-plugin` example.

Another performance consideration is that vertex attributes are interleaved by default on the GPU in the vertex buffer. This means that updating the position of all vertices results in a non-blittable transfer. This could result in a performance loss. Unity exposes some control over the vertex buffer layout allowing separation of vertex attributes into separate 'streams'. This could be explored further if this process appears to be a performance bottleneck.

## Frontend

### VR Interface

Oculus provides an [Oculus Integration](https://assetstore.unity.com/packages/tools/integration/oculus-integration-82022) :cite:`oculus-integration` on the Unity Asset Store to provide common functionality. However, since Unity 2019.3 there has been a [Unity XR Plug-in Framework](https://docs.unity3d.com/Manual/XRPluginArchitecture.html) :cite:`xr-plugin-framework` package to simplify the interface with each of the SDKs of the VR platforms. Additionally, there is a [Unity XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html) :cite:`xr-interaction-toolkit` package with provides cross-platform input as well as common VR functionality such as locomotion and interaction with UI. These packages are the preferred option over the Oculus Integration and will ensure that the application can be used on most VR devices.

#### Locomotion

This involves moving the user in the virtual world. The common approach is to use teleportation with a curved ray to translate the user and snap rotation to turn user. Using a curved ray for indicating where to translate to has the benefit that there is an approximately linear mapping between the angle of the controller and the distance up to a certain point. This makes is easy to precisely indicate a teleportation position far away from the user. Immediate locomotion rather than a smooth interpolation is used to reduce motion sickness.

### Input

The [Unity XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@0.9/manual/index.html) :cite:`xr-interaction-toolkit` is used for this for getting cross-platform input. It is also used for detecting the controllers. 

.. :: Mapping from input to actions

.. :: Future: left/right handed, notion of using a primary and secondary hand

#### Contextual Input

Contextual input is when we adapt what the mapping of raw inputs to actions based on the context or state and is important for two reasons. Firstly, a key feature of making input intuitive is by making it adapt to the current context. Secondly, contextual input helps to make maximum use of the limited degrees of freedom provided by the physical input device. When using a keyboard this has never been particularly important for smaller applications. 

Context can be inferred from the state of the application or explicitly set by the user, for example when choosing a specific tool. Ideally most of the context is inferred implicitly from what the user is doing. For example, while the user is grabbing a selection we want to provide relevant further input options to that context, such as being able to change the pivot mode or whether rotation is enabled. Having different tools for editing a single mesh and operating with multiple meshes was inspired by the object and edit modes in Blender :cite:`blender`

In this project, the state is inferred using a tree, [see documentation](https://vr-modeling.readthedocs.io/docs/developer-guide/adding-functionality.html#customizing-input) :cite:`docs-customizing-input`. This allows for easily determining the context. However, it is unclear how well this will scale once the state space increases. This is an area which could be improved in the future.

### User Interface

#### Overview

Despite this being a VR application, some form of a 2D User Interface (UI) is still necessary. It allows for displaying information as well as providing access to infrequently used actions. Frequently used actions should ideally be mapped directly to contextual controller input. However, if all degrees of freedom in the controller input are already used, then the 2D UI will be the fallback option.

The UI is implemented as a world-space canvas. Using a traditional screen space UI is not an option in VR. This creates the problem of positioning of the UI. The relevant UI needs to be displayed at the right time for an intuitive experience. In this project, the user can grab UI panels and position them explicitly, similar to the Unreal Engine VR Mode. In future, methods of implicitly positioning the UI and displaying relevant parts may work better. Also having the user grab a panel by default when it is created to let them set the initial position will be an improvement, as currently panels are simply arranged in an array-like fashion.

#### Generation

In order to be able to add new functionality easily, generating UI via C# scripting is done. The goal is to be able to easily add new UI elements and configure them, in particular setting their `OnClick` behavior. Inspired by the 2D libigl UI, we simply have a scrollable vertical layout group, so any child is then automatically formatted. 

For this the base UI elements are created in the editor and saved as prefabs. If advanced functionality is required a :cs:class:`MonoBehaviour` component is added. Once this preparation is done for several UI elements, one can instantiate these via script and access their components to customize them. This method has proven to be effective in terms of easy expansibility. 

#### Performance

After initial performance profiling, a significant amount (>50%) of the frame time was spent raycasting the UI elements. This affected frame rates significantly leading to jitter. To reduce this a straight ray is used for the UI, as curved rays are implemented by using several straight raycasts. Additionally when the UI canvas is not being hovered over by the ray, the UI graphics raycaster is disabled. This works by the assumption that all interactable UI elements are contained inside the canvas, which is not a strict requirement with a Unity world-space canvas.

As the number of UI elements increases in the future, there will most likely need to be further UI performance optimizations. For example, occlusion culling for raycasting not just rendering. It is unclear whether this is done by default. The newer xml/css based [Unity UI Toolkit](https://docs.unity3d.com/2020.1/Documentation/Manual/UIElements.html) :cite:`ui-toolkit` will likely solve many of these issues once it becomes a verified package with runtime support.

#### Tooltips and Input Hints

To make the application more intuitive and user friendly, we need a way of providing the user with relevant help information when required. The intent being that a user learns how to use a feature when they need it, colloquially just-in-time learning. This requires inferring of the context, similarly to the input context. To solve this tooltips are provided to display a short text when hovering over a UI element. 

Input hints tell the user what each button/axis does and are displayed over the controller based on the input context, see gallery.

#### Alternatives

In order to rely less on UI, other input methods are also possible. Speech recognition is an example which was attempted with the :cs:class:`KeywordRecognizer` :cite:`speech-keyword-recognizer`. This is however still too unreliable and unresponsive, often with a delay of more than one second. However if improved, speech could be used effectively for certain actions. 

Controller gestures and pie menus also present potentially fast methods of interacting by making use of the positional input. Using pie menus for numerical input with the joysticks may also be worth exploring in the future. 

### High Level Actions

#### Vertex Selection

Vertex selections are used for affecting only parts of the mesh or as an input to a libigl function. A key feature is being able to transform selections with the controllers, as well as being able to transform two selections with each hand independently. This requires that we have multiple selections simultaneously. To solve this efficiently a bitmask is used. Each vertex has an additional 32-bits (represented as an integer), with each bit indicating whether it is in the selection or not. This allows for up to 32 selections, which is reasonable for this use case. 

An additional benefit of using bitmasks is that we can provide a mask of selections with one integer. For example, we can choose which selections are visible or will be translated with an integer. If we want to affect all selections we simply use the maximum integer value, where all bits are one. Functions that act on a selection have been modified, if possible, to act on a mask of selections.

As Eigen does not currently have cwise bitwise operations, unary functions were used. These might not be as well optimized. However, when testing on the [armadillo mesh](http://graphics.stanford.edu/data/3Dscanrep/) :cite:`standford-3d-models` the interactions was still responsive enough.

Face or edge selection was not implemented as this is more involved and does not necessarily add more features for the current use case.

#### Transformations

In order to transform a mesh or part of the mesh there are two stages: finding which transformation should be done and how to apply it. For determining an affine transformation - translation, rotation, scaling - we are much more flexible in VR, as we have two controllers. Some inspiration was taken from Quill :cite:`quill` for how the transformations behave.

Once the transformation is known we can either apply it to the mesh directly, which is done in C# with the Unity API. Or we can apply it to a vertex selection mask, which is done in C++ and modifies the vertex data. This implementation is more involved as transforming a selection mask needs to be done on the worker thread. It uses the [Eigen geometry module](https://eigen.tuxfamily.org/dox/group__Geometry__Module.html) :cite:`eigen-geometry`.

When working with multiple meshes or multiple selections, we need to determine what to transform – a mask of meshes or selections. For this the sphere brush is used. If a mesh or selection is inside, it is affected. If there is nothing inside then the active mesh or selection is affected. This provides lots of control but also gives an intuitive experience. If both hands act on the same mask then we perform two handed transformations, such as scaling. This method provides and simple way for operating on multiple selections.

To provide more fine grained control, the amount by which the grip buttons are pressed is used as a smoothing factor. This works well, although it can be hard to control this smoothing factor precisely. It may make sense to apply a log or square root to the smoothing factor to counteract this. 

Different pivot modes where tested: mesh center, selection center and hand center. Using the hand as the center appeared the most natural. For transforming selections, using the mesh as the center usually gave unintuitive results, particularly for smaller selections, see gallery.

#### Deformations

The libigl biharmonic deformation `igl::harmonic` can be toggled on. If enabled it will be run whenever the input arguments have been changed. In this case, when the boundary conditions have changed. This can be detected quite easily by checking the :cs:var:`DirtyState` of the mesh data have been modified when applying the mesh data in :cpp:func:`ApplyDirty`. The As-Rigid-As-Possible `igl::arap` deformation works very similarly, except that we need to check when the precomputation needs to be done. For details as well as diagrams see the [documentation](https://vr-modeling.readthedocs.io/docs/developer-guide/adding-functionality.html#custom-deformation) :cite:`docs-custom-deformation`.

## Documentation Process

There are several parts to the documentation process, all of which need to be equally addressed. It is important to make a distinction for how to:

1. Use the end product
1. Start development and understand the overall process
1. Use the existing code/API
1. Understand the development approach and an evaluation of the project (this report)

An important part is also that the documentation should be inlined as much as possible, so that it is made part of the source. This means it can be easily found when developing and that it is more easily maintained. 

Most functions and types have an annotated docstring, in C# a xml-doc and javadoc in C++. This provides information on how to *use* the function/type. In the implementation, there are comments as required for how to *modify* the function/type. As in C# everything resides inside a class/struct/interface the docstring of the class is intended to give an overview of everything inside and its intention.

Additional markdown files are there to add an overview and provide general information not specific to a file or piece of code. These files are placed 'inline' next to the `.cs` or `.cpp` files. Within these, diagrams.net is used for flowcharts.

To compile all this information, [Doxygen](https://www.doxygen.nl) :cite:`doxygen` and [Sphinx](https://www.sphinx-doc.org) :cite:`sphinx` are used. Doxygen is used to extract the documentation from the code. This information in xml format is then used by Breathe (a Sphinx extension) to render it with Sphinx, which then combines it with the markdown files. [Breathe](https://github.com/michaeljones/breathe) :cite:`breathe` and the language domains ensure cross-referencing of items. For this to work with C#, the [sphinx-csharp](https://github.com/djungelorm/sphinx-csharp) :cite:`sphinx-csharp` and breathe projects where modified, see [#8](https://github.com/djungelorm/sphinx-csharp/pull/8) and [#550](https://github.com/michaeljones/breathe/pull/550) respectively on GitHub.

[ReadTheDocs](https://readthedocs.org) :cite:`read-the-docs` is used to host and compile the website output of Sphinx. This has continuous integration. Whenever a commit is pushed to the `read-the-docs` branch, the website is recompiled. 

## Miscellaneous Features

1. Importing of meshes into Unity, adjusting scaling, vertex buffer layout and materials
   1. Recognized file types use an asset post-processor
   1. Custom file types, e.g. `.off`, are imported via libigl within a scripted importer
   1. Meshes are validation before instantiation
1. UI to indicate when a thread is inside `Execute` for a longer time
1. Rendering with the Universal Render Pipeline (URP)
1. Environment modelled in Blender, ocean shader created using Shader Graph
1. Speech recognition for specific actions (disabled by default)
1. Cross-platform controller models
1. Different modes for editing selections: add, remove, invert, new/clear selection per stroke
1. Conversion of selection mask to vertex color

For more features see the documentation and repository.

.. bibliography:: references.bib
   :filter: docname in docnames
   :style: unsrt
   :labelprefix: 2.