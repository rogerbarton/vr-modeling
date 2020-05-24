#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <RenderAPI/RenderAPI.h>
#include <string>
#include "State.h"

// A global variable should be extern, so it can be seen in several cpp's. 
// It is then defined in the Interface.cpp once
extern StringCallback DebugLog;
extern StringCallback DebugLogWarning;
extern StringCallback DebugLogError;

extern IUnityInterfaces* s_UnityInterfaces;

extern "C" {
    // Interface.cpp
    UNITY_INTERFACE_EXPORT void Initialize(StringCallback debugCallback, StringCallback debugWarningCallback, StringCallback debugErrorCallback);
    UNITY_INTERFACE_EXPORT State* InitializeMesh(const UMeshDataNative data, const char* name);
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
    UNITY_INTERFACE_EXPORT void ApplyDirty(State* state, const UMeshDataNative data);

    // ModifyMesh.cpp
    UNITY_INTERFACE_EXPORT void TranslateMesh(State* state, Vector3 value);
    UNITY_INTERFACE_EXPORT void TransformSelection(State* state, int selectionId,
    		Vector3 translation, float scale, float angle, Vector3 axis);
    UNITY_INTERFACE_EXPORT void Harmonic(State* state);

    // Selection.cpp
    UNITY_INTERFACE_EXPORT void SphereSelect(State* state, Vector3 position, float radiusSqr, int selectionId = 0, int selectionMode = SelectionMode::Add);
    UNITY_INTERFACE_EXPORT void ClearSelection(State* state, int selectionId = -1);
    UNITY_INTERFACE_EXPORT void SetColorBySelection(State* state, int selectionId = -1);
}
