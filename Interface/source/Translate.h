#include <IUnityInterface.h>

extern "C" {
    struct Vector3
    {
        float x;
        float y;
        float z;
    };

    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr);

}