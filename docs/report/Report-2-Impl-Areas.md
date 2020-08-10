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

There are two parts to consider when adding VR functionality. The input and the output (rendering). The input part varies greatly between different VR devices and platforms, even with newer devices like the Oculus Quest having hand tracking in contrast to controllers. Initially this project was aimed purely at the Oculus Rift S. Oculus provides an Oculus Integration on the Unity Asset Store to provide this functionality. However, since around Unity 2019.3 there has been a Unity VR Plugin system to simplify the interface with each of the SDKs of the VR platforms. Additionally, there is a Unity XR Interaction Toolkit package with provides cross-platform input as well as common VR functionality such as locomotion and interaction with UI. This package is the preferred option over the Oculus Integration.

### Locomotion

This involves moving the user in the virtual world. The common approach here is to use teleportation with a curved ray to translate the user and snap rotation to adjust the rotation of the user. Using a curved ray for indicating where to translate to has the benefit that there is an approximately linear mapping between the angle of the controller and the distance. This makes is easy to precisely indicate a teleportation position far away from the user.

### Interaction with UI



### Interaction with Objects

Interaction with libigl meshes will be discussed later in section ???.

## User Interface

Despite this being a VR application 2D User Interface (UI) is still necessary. It allows for displaying information as well as providing access to infrequently used actions. Frequently used actions should ideally be mapped to contextual controller input. However, if all degrees of freedom in the controller input are already used, then the 2D UI will be the fallback option.

The UI in VR is done as a world space canvas. Using a traditional screen space UI is not an option. 

### Interaction

Allowing the user to position the UI is a essential feature. Thus the UI object have been made grabbable objects (`XRGrabbable`) with an offset transform. 

Interaction with the UI relies on raycasting, as this is the most intuitive method to select UI elements.

### Generation

In order to be able to add new functionality easily, generating UI via C# scripting is necessary. The goal is to be able to easily add new UI elements and configure them, in particular setting their on click behavior. For this we simply have a scrollable vertical layout group, so any child is then automatically formatted.

For this the base UI elements were created in the editor and saved as prefabs. If advanced functionality was required a `MonoBehaviour` component was added with a simple interface. Once this preparation is done for several UI elements, once can instantiate these via script and access their components to customize them. This method has proven to be quite effective in terms of easy expansibility. 

### Performance

After performance profiling, a significant amount (>50%) of the frame time was spent raycasting the UI elements. This affected frame rates significantly leading to jitter. To reduce this a straight ray is used for the UI, as curved rays are implemented by using several straight raycasts. Additionally if the UI canvas is not being hovered over by the ray, then the UI graphics raycaster is disabled.

### Tooltips and Input Hints

To make the application more intuitive and easy to use, we need a way of providing the user with relevant help information when required. Tooltips are provided to display a short text when hovering over a UI element. Input hints tell the user what each button/axis does when pressed. Input hints are displayed directly on the controller. These can be disabled if desired, with the joystick click.

## Input

The Unity XR Interaction Toolkit is used for this for cross-platform input.

### Contextual Input

Contextual input is when we adapt what the input does based on the context or state and is important for two reasons. Firstly, a key feature of making input intuitive is by making it adapt to the current context. Secondly, contextual input helps to make maximum use of the degrees of freedom provided by the physical input device. 

Context can be inferred from the state of the application and from the input itself. In this application, two 'tools' are for manipulating objects or selections provided. These are essentially two states that we can use for inferring what the user would like to do. This can be considered as an *explicit* context as the user must explicitly set this, by choosing the tool. Ideally the context is inferred implicitly from what the user is doing. 

For example, when the user is grabbing a selection we want to provide relevant input to that context such as being able to change the pivot mode or whether rotation is enabled.

## Threading

In order to have a high framerate, the expensive computations done by libigl must be done on a worker thread. The Unity API, such as getting the transform position, is not thread-safe and thus use from a worker thread is forbidden. This has several implications, with the main one being that all access to the Unity API must be done before starting the thread and the results should be copied. This is what :cs:func:`PreExecute` and the :cs:class:`MeshState` are for. If the thread wants to make changes to the Unity state, e.g. moving an object, then this must be deferred to the main thread. Here this is done once the thread has finished in :cs:func:`PostExecute`, however, a concurrent queue of actions could also be used.

As we want to execute certain operations every frame and apply their changes, we have a loop of `PreExecute`, `Execute` and `PostExecute`. Where `PreExecute` and `PostExecute` are performed on the main thread. Notably, in `PostExecute` we apply changes to the mesh done by libigl.

C# threads are used.

## C#/C++ Interface and Data Layout

To call libigl functions a necessary C#/C++ language interface is required. This adds an extra layer of complexity. We must consider in which language functionality and data resides and what is shared. An important note is that the Unity API is only accessible within C#.

It is important to have a clear distinction of what is done where. All expensive mathematical operations are generally done in C++. The number of interface functions is kept minimal. All input is done in C# and passed as arguments.

In data we also have a distinction. Shared datatypes must be declared in both languages, including Unity types such as :cs:class:`Vector3`. The C# garbage collector also needs to be considered. Within a function this is not a problem. Data is `fixed` within the scope of a function. However, is a C# pointer is passed to C++ and this pointer is used after the function scope, then the memory may have moved and the pointer is no longer valid. This data should be pinned with :cs:func:`GcHandle.Alloc`. To avoid this scenario persistent data is allocated, and thus deleted, in C++, for example the :cpp:class:`MeshState`.

### Unity Plugin Reloading

Unity presents a complication that it does not unload libraries once they are loaded, which happens when it is first used. This means that we cannot recompile the C++ library without restarting Unity. This creates a larger iteration time. In order to counter this, the UnityNativeTool open source project is used. This effectively wraps native functions and un/loads the library itself. It is an editor-only tool. A few modifications were made to this in several pull requests, see #14, #15, #18, #19, #20, #21, #28 on GitHub. 

## Mesh Interface

Once we have modified the mesh data that is used by the renderer, such as the vertex positions, we need to apply these changes similar to `viewer.data().set_vertices(V)` in the 2D viewer. This requires access to the Unity API so must be done on the main thread. This is done in `PostExecute`. A bitmask :cs:class:`DirtyState` is used to indicate which parts have been modified and need to be updated.

An extra complication to this is that Unity used row-major and libigl expects column-major. Because of this we have two copies of the data, one in column-major and one in row-major. This creates a necessary transpose each time we apply changes, which is done in C++ on the worker thread.

### Performance

The performance of the current method seems to be good enough. Unity provides the GPU pointer to the mesh buffer. Thus a way of applying the mesh data directly to the GPU was briefly explored.

Ideally libigl would work equally well in row-major preventing a transpose and reducing the number of copies of the mesh in memory. Although Eigen supports row-major well, libigl templates do not always consider this.

## High Level Actions



## Documentation Process

There are several parts to the documentation process, all of which need to be equally addressed. It is important to make a distinction for how to:

1. Use the end product
1. Start development (code overview)
1. Use the existing code/API

An important part is also that the documentation should be inlined as much as possible, so it can easily be found and is made part of the source.

Most functions and types have an annotated docstring, in C# a xml-doc and javadoc in C++ so that the IDEs can display this nicely. This provides information on how to *use* the function/type. In the implementation, there are comments as required for how to *modify* the function/type. As in C# everything resides inside a class/struct/interface the docstring of the class is intended to give an overview of everything inside and its intention.

Additional markdown files are there to add an overview of the files and provide general information not specific to a file or piece of code. These files are placed 'inline' next to the `.cs` or `.cpp` files. 

To condense all this information, Doxygen and Sphinx are used. Doxygen is used to extract the documentation from the code. This information in xml format is then used by Breathe (a Sphinx extension) to render it with Sphinx, which then combines it with the markdown files. Breathe and the language domains ensure cross-referencing of items. 

For this to work with C#, the *sphinx-csharp* and *breathe* projects where modified, see #8 and #550 respectively on GitHub.

ReadTheDocs is used to host and compile the website output of Sphinx. This has continuous integration. Whenever a commit is pushed to the `read-the-docs` branch, the website is recompiled.

