#pragma once
#include <PluginAPI/IUnityInterface.h>

// Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);

struct Vector3
{
    float x;
    float y;
    float z;
};