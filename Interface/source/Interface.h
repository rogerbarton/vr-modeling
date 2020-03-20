#pragma once
#include <Unity/IUnityInterface.h>
#include <Unity/RenderAPI.h>
#include <string>
#include "InterfaceTypes.h"

// Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);
// A global variable should be extern, so it can be seen in several cpp's. 
// It is then defined in the Interface.cpp once
extern StringCallback DebugLog;
extern std::string modelRoot;
extern IUnityInterfaces* s_UnityInterfaces;
extern IUnityGraphics* s_Graphics;

struct VertexUploadData{
    int changed;
    float* gfxVertexBufferPtr;
    float* VPtr;
    int VSize;
};

extern "C" {
    UNITY_INTERFACE_EXPORT void Initialize(const char* modelRootp, StringCallback debugCallback);
    UNITY_INTERFACE_EXPORT void UnityPluginLoad(IUnityInterfaces* unityInterfaces);
    UNITY_INTERFACE_EXPORT void UnityPluginUnload();
    UNITY_INTERFACE_EXPORT void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

    //IO.cpp
    UNITY_INTERFACE_EXPORT void LoadOFF(const char* path, const float scale, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize);

    // Translate.cpp
    UNITY_INTERFACE_EXPORT UnityRenderingEventAndData GetUploadMeshPtr();
    UNITY_INTERFACE_EXPORT void UploadMesh(int eventId, void* data);
    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 value);
}