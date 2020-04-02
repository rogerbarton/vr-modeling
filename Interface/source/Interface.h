#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <RenderAPI/RenderAPI.h>
#include <string>
#include "InterfaceTypes.h"

// A global variable should be extern, so it can be seen in several cpp's. 
// It is then defined in the Interface.cpp once
extern StringCallback DebugLog;

extern IUnityInterfaces* s_UnityInterfaces;

extern "C" {
    //Interface.cpp
    UNITY_INTERFACE_EXPORT void Initialize(StringCallback debugCallback);

    //Unity Callbacks
    UNITY_INTERFACE_EXPORT void UnityPluginLoad(IUnityInterfaces* unityInterfaces);
    UNITY_INTERFACE_EXPORT void UnityPluginUnload();

    //Custom Upload to GPU
    UNITY_INTERFACE_EXPORT UnityRenderingEventAndData GetUploadMeshPtr();
    UNITY_INTERFACE_EXPORT void UploadMesh(int eventId, void* data);

    //IO.cpp
    UNITY_INTERFACE_EXPORT void LoadOFF(const char* path, const float scale, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize);
    UNITY_INTERFACE_EXPORT void TransposeInPlace(void* MatrixPtr, int cols);

    //Translate.cpp
    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 value);
    UNITY_INTERFACE_EXPORT void Harmonic(float* VPtr, int VSize, int* FPtr, int FSize);
}