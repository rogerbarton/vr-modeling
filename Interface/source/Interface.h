#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <RenderAPI/RenderAPI.h>
#include <string>
#include "InterfaceTypes.h"

// A global variable should be extern, so it can be seen in several cpp's. 
// It is then defined in the Interface.cpp once
extern StringCallback DebugLog;
// Macro to easily concat strings using stringstream, use the operator<<
#define STR(message) static_cast<std::ostringstream &&>((std::ostringstream() << message)).str().data()
// Macro to easily print to the Unity Debug.Log
#define LOG(message) if(DebugLog) { DebugLog(STR(message)); }

extern IUnityInterfaces* s_UnityInterfaces;

extern "C" {
    // Interface.cpp
    UNITY_INTERFACE_EXPORT void Initialize(StringCallback debugCallback);
    UNITY_INTERFACE_EXPORT State* InitializeMesh(const MeshDataNative data, const char* name);
    UNITY_INTERFACE_EXPORT void DisposeMesh(State* state);

    // Unity Callbacks from IUnityInterface.h
    // UNITY_INTERFACE_EXPORT void UnityPluginLoad(IUnityInterfaces* unityInterfaces);
    // UNITY_INTERFACE_EXPORT void UnityPluginUnload();

    // Custom Upload to GPU
    // UNITY_INTERFACE_EXPORT UnityRenderingEventAndData GetUploadMeshPtr();
    // UNITY_INTERFACE_EXPORT void UploadMesh(int eventId, void* data);

    // IO.cpp
    UNITY_INTERFACE_EXPORT void LoadOFF(const char* path, const bool setCenter, const bool normalizeScale, const float scale,
    		void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize);
    UNITY_INTERFACE_EXPORT void TransposeInPlace(void* MatrixPtr, int rows, int cols = 3);
    UNITY_INTERFACE_EXPORT void TransposeTo(void* InMatrixPtr, void* OutMatrixPtr, int rows, int cols = 3);

    // ModifyMesh.cpp
    UNITY_INTERFACE_EXPORT void TranslateMesh(float* VPtr, int VSize, Vector3 value);
    UNITY_INTERFACE_EXPORT void Harmonic(float* VPtr, int VSize, int* FPtr, int FSize);
    UNITY_INTERFACE_EXPORT void SphereSelect(State* state, const MeshDataNative udata, Vector3 position, float radius);

    // Sample.cpp
    UNITY_INTERFACE_EXPORT void CustomUpdateSample(const MeshDataNative data, unsigned int& dirtyState);
}
