# Unity Mesh API

This is about how meshes are passed to Unity and updated so that they can be rendered.

Since Unity 2019.3 there have been some updates to the mesh API. However, there are still various limitations and currently the older API is being used as it is much simpler and thus more reliable. In this application, native arrays allocated in the C++ dll are used.

Some useful points:

- `mesh.MarkDynamic()` on a *readable* mesh to keep a copy of the buffers on the managed CPU memory and make `mesh.vertices` read/writable. This is required in order to be able to modify the vertices at runtime.
- `mesh.SetVertexBufferParams()` to specify the layout on the GPU, attributes in the same stream are interleaved. Here you can specify the precision as well.
- `mesh.UploadData(false)` to copy the `mesh.vertices` managed data to the GPU immediately (else done pre-render), using `true` will delete the CPU copy and the mesh will no longer by dynamic/writable.
- `mesh.GetNativeVertexBufferPtr()` gets the GPU (DirectX/OpenGL) pointer

## Further Reading

1. [Unity Docs - Mesh](https://docs.unity3d.com/ScriptReference/Mesh.html)
1. [Unity Docs - Mesh.SetVertexBufferData](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferData.html)
1. [Unity Docs - Mesh.SetVertexBufferParams](https://docs.unity3d.com/ScriptReference/Mesh.SetVertexBufferParams.html)
1. [Sample from GraphicsDemos](https://bitbucket.org/Unity-Technologies/graphicsdemos/pull-requests/2/example-of-native-vertex-buffers-for/diff)
1. [Mesh API Feedback Forum](https://forum.unity.com/threads/feedback-wanted-mesh-scripting-api-improvements.684670/) with link to [Google Docs](https://docs.google.com/document/d/1I225X6jAxWN0cheDz_3gnhje3hWNMxTZq3FZQs5KqPc/edit)
1. [NativeArray use cases](https://gamedev.stackexchange.com/questions/174953/unity-uses-for-nativearray/174956#174956?newreg=ee4ce68f58c540479161bad1841be246)

