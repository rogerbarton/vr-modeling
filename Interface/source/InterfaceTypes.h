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

/**
 * Stores all pointers to the MeshData arrays.<p>
 * Usually this should be as a <code>const</code> parameter
 */
struct MeshDataNative
{
    float* V;
    float* N;
    float* C;
    float* UV;
    int* F;

    int VSize;
    int FSize;
};

/**
 * Marks which data has changed in <code>MeshDataNative</code> as a bitmask
 */
enum DirtyFlag : unsigned int {
    None = 0,
    All = 0xFFFFFFFF,
    VDirty = 1,
    NDirty = 2,
    CDirty = 4,
    UVDirty = 8,
    FDirty = 16,
};
