# Unity Mesh API

*This is about how meshes are updated from C++ Eigen matrices to Unity, so that they can be rendered.*

Since Unity 2019.3 there have been some updates to the mesh API. However, there are still various limitations and *currently the older API (*`SetVertices`*) is being used* with :cs:class:`NativeArray` as it is much simpler and thus more reliable. The flowchart below shows the different copies of the mesh data (e.g. vertex position matrix V). 

<iframe frameborder="0" style="width:100%;height:230px;" src="https://app.diagrams.net/?lightbox=1&highlight=0000ff&layers=1&nav=1&title=mesh-storage#Uhttps%3A%2F%2Fdrive.google.com%2Fuc%3Fid%3D1qBMcfZnqcMWeAa0_NpIMQEWz5YlmmIOj%26export%3Ddownload"></iframe>

Indeed there are **4 copies of the mesh** currently. Potentially, the 'CPU Unity internal' copy does not exist, but this is unclear. Libigl currently only reliably supports column-major, but Unity requires row-major data. A necessary transpose is required. In this case it is most efficient to have two copies.

Note that updates to the mesh only occur when a :cs:class:`DirtyFlag` is set in the :cs:var:`MeshState.DirtyState`. :cs:class:`DirtyFlag` are propagated and cleared when processed. The native :cpp:func:`ApplyDirty` is called by the managed :cs:func:`ApplyDirty` in :cs:class:`UMeshData`.

It is also important to note that the transposing in :cpp:func:`ApplyDirty` is done on the worker thread. `mesh.SetVertices()` must be called on the main thread (as it is a Unity API). It is called by the managed :cs:func:`ApplyDirtyToMesh` function in :cs:class:`UMeshData`.

Some useful points when working with Unity meshes:

- Call `mesh.MarkDynamic()` on a *readable* mesh to keep a copy of the buffers on the managed CPU memory and make `mesh.vertices` read/writable. This is required in order to be able to modify the vertices at runtime. The mesh must be marked as writable in the import settings in the Inspector.
- Call `mesh.SetVertexBufferParams()` to specify the layout on the GPU, attributes in the same stream are interleaved. Here you can specify the precision as well. See :cs:var:`VertexBufferLayout` in `Native.cs`.
- `mesh.UploadData(false)` to copy the `mesh.vertices` 'CPU Unity internal' managed data to the GPU immediately (else done pre-render), using `true` will delete the CPU copy and the mesh will no longer by dynamic/writable.
- `mesh.GetNativeVertexBufferPtr()` gets the GPU (DirectX/OpenGL) pointer
  - Potentially with this we can apply changes to the GPU copy directly, potentially gaining performance. This has been experimented with in the `source/CustomUploadMesh.cpp`

## Further Reading

1. [Unity Docs - Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html)
1. [Unity Docs - Mesh.SetVertexBufferData](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferData.html)
1. [Unity Docs - Mesh.SetVertexBufferParams](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferParams.html)
1. [Sample from GraphicsDemos](https://bitbucket.org/Unity-Technologies/graphicsdemos/pull-requests/2/example-of-native-vertex-buffers-for/diff)
1. [Mesh API Feedback Forum](https://forum.unity.com/threads/feedback-wanted-mesh-scripting-api-improvements.684670/) with link to [Google Docs](https://docs.google.com/document/d/1I225X6jAxWN0cheDz_3gnhje3hWNMxTZq3FZQs5KqPc/edit)
1. [NativeArray use cases](https://gamedev.stackexchange.com/questions/174953/unity-uses-for-nativearray/174956#174956?newreg=ee4ce68f58c540479161bad1841be246)

