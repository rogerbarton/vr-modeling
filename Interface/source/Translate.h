#include <IUnityInterface.h>
#include "InterfaceTypes.h"

extern "C" {
    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr);
}