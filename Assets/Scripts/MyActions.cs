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
        UIActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate, "Test",
            _ => { Debug.Log("Execute Test"); },
            () => Input.GetKeyDown(KeyCode.Q),
            default, default,
            new[] {"Test"},
            0));

        UIActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate, "Translate",
            new TranslateAction(),  
            new[] {"move", "translate"},
            1));

        UIActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate, "Smooth",
            data =>
            {
                unsafe
                {
                    Native.Harmonic((float*) data.V.GetUnsafePtr(), data.VSize, (int*) data.F.GetUnsafePtr(),
                        data.FSize);
                }

                data.DirtyState |= MeshData.DirtyFlag.VDirty;
            }, 
            () => Input.GetKeyDown(KeyCode.E),
            default, default,
            new[] {"smooth", "harmonic", "laplacian"},
            2
            ));

        UIActions.get.RegisterAction(new MeshAction(MeshActionType.OnUpdate, "CustomUpdate",
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
