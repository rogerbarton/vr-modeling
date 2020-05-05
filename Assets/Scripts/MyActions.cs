using libigl;
using libigl.Samples;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

/// <summary>
/// Contains all user action instances
/// </summary>
public class MyActions : MonoBehaviour
{
    private void Awake()
    {
        // Add all actions
        MeshActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate,
            "Test",
            new[] {"Test"},
            0,
            () => Input.GetKeyDown(KeyCode.Q),
            _ => { Debug.Log("Execute Test"); }));

        MeshActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate,
            "Translate",
            new[] {"move", "translate"},
            1,
            new TranslateAction()));

        MeshActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate,
            "Smooth",
            new[] {"smooth", "harmonic", "laplacian"},
            2,
            () => Input.GetKeyDown(KeyCode.E),
            data =>
            {
                unsafe
                {
                    Native.Harmonic((float*) data.V.GetUnsafePtr(), data.VSize, (int*) data.F.GetUnsafePtr(),
                        data.FSize);
                }

                data.DirtyState |= MeshData.DirtyFlag.VDirty;
            }));

        MeshActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate, "CustomUpdate",
            data =>
            {
                // Example where everything is done in C++, we can also pass additional arguments about the input state if we wanted
                // This will be called "every frame" unless there is already some computation running
                var tmp = (uint) data.DirtyState;
                Native.CustomUpdateSample(data.GetNative(), ref tmp);
                data.DirtyState = (MeshData.DirtyFlag) tmp;
            },
            () => false)); // Change this to true to test sample
    }
}
