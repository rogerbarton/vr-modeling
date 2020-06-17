#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <RenderAPI/RenderAPI.h>
#include <string>
#include "State.h"

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
    UNITY_INTERFACE_EXPORT void ApplyDirty(State* state, const UMeshDataNative data, const unsigned int visibleSelectionMask);

    // ModifyMesh.cpp
    UNITY_INTERFACE_EXPORT void TranslateMesh(State* state, Vector3 value);
    UNITY_INTERFACE_EXPORT void TranslateSelection(State* state, Vector3 value, int selectionId = -1);
    UNITY_INTERFACE_EXPORT void TransformSelection(State* state, int selectionId,
    		Vector3 translation, float scale, Quaternion rotation);
    UNITY_INTERFACE_EXPORT void Harmonic(State* state, unsigned int boundaryMask = -1, bool showDeformationField = true);
    UNITY_INTERFACE_EXPORT void Arap(State* state, unsigned int boundaryMask = -1);
    UNITY_INTERFACE_EXPORT void ResetV(State* state);

    // Selection.cpp
    UNITY_INTERFACE_EXPORT void SelectSphere(State* state, Vector3 position, float radius, int selectionId = 0, unsigned int selectionMode = SelectionMode::Add);
    UNITY_INTERFACE_EXPORT unsigned int GetSelectionMaskSphere(State* state, Vector3 position, float radius);
    UNITY_INTERFACE_EXPORT void ClearSelection(State* state, int selectionId = -1);
    UNITY_INTERFACE_EXPORT void ClearSelectionMask(State* state, unsigned int maskId);
    UNITY_INTERFACE_EXPORT void SetColorBySelection(State* state, int selectionId = -1);
    UNITY_INTERFACE_EXPORT void SetColorByMask(State* state, unsigned int maskId = -1);
}
