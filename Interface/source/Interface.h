#pragma once
#include <IUnityInterface.h>
#include <string>
#include "InterfaceTypes.h"

//Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);
StringCallback DebugLog;

extern "C" {
    UNITY_INTERFACE_EXPORT void Initialize(const char* modelRootp, StringCallback debugCallback);

    // Translate.cpp
    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr);
}