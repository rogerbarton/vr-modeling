#pragma once
#include <Unity/IUnityInterface.h>

// Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);

struct VertexUploadData {
    int changed;
    float* gfxVertexBufferPtr;
    float* VPtr;
    int VSize;
};

struct Vector3
{
    float x;
    float y;
    float z;
};