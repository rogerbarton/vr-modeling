#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <RenderAPI/RenderAPI.h>
#include <string>
#include "State.h"

namespace Interface
{
	extern "C"
	{
	// --- Interface.cpp
	/**
	 * Called just before the first function is called from this library.
	 * Use this to pass initial data from C#
	 * @see C# Native.Initialize()
	 * @param debugCallback A C# delegate (function pointer) to print to the Unity Debug.Log
	 */
	UNITY_INTERFACE_EXPORT void
	Initialize(StringCallback debugCallback, StringCallback debugWarningCallback, StringCallback debugErrorCallback);
	/**
	 * Called when a new mesh is loaded. Initialize global variables, do pre-calculations for a mesh
	 * @param data The Unity MeshData
	 * @param name Name of the mesh
	 * @return A pointer to the C++ state for this mesh
	 */
	UNITY_INTERFACE_EXPORT State* InitializeMesh(const UMeshDataNative data, const char* name);
	/**
	 * Disposes all C++ state tied to a mesh properly
	 */
	UNITY_INTERFACE_EXPORT void DisposeMesh(State* state);

	// --- Unity Callbacks from IUnityInterface.h
	/**
	 * Called when the plugin is loaded, this can be after/before Initialize()<p>
	 * Declared in IUnityInterface.h
	 * @param unityInterfaces Unity class for accessing minimal Unity functionality exposed to C++ Plugins
	 */
	UNITY_INTERFACE_EXPORT void UnityPluginLoad(IUnityInterfaces* unityInterfaces);
	/**
	 * Called when the plugin is unloaded, clean up here
	 * Declared in IUnityInterface.h
	 */
	UNITY_INTERFACE_EXPORT void UnityPluginUnload();

	// --- IO.cpp
	/**
	 * Propagates changes from the libigl mesh (Eigen matrices) to the Unity NativeArrays so we can apply it to the mesh in C#.
	 * This functions also recalculated selection sizes, colors based on the selection.
	 * This should be called once after apply all wanted changes to the libigl mesh.
	 * This function itself does not modify the Unity mesh, see UMeshData.cs
	 * @param data Pointers to the Unity Mesh Data, where to apply changes to.
	 * @param visibleSelectionMask Selections which are currently visible, will ignore changes to invisible selections.
	 */
	UNITY_INTERFACE_EXPORT void
	ApplyDirty(State* state, const UMeshDataNative data, const unsigned int visibleSelectionMask);
	/**
	 * Reads an .off file into **row major** Eigen matrices, these can then be mapped by a NativeArray in C#.
	 * Matrices are allocated with <code>new</code> and must be deleted manually
	 * (e.g. by <code>NativeArray<T>.Dispose()</code> or converting with <code>Allocator.Temp</code>).
	 * @param path The asset path, absolute or relative to the project root e.g. AssetImportContext.assetPath or "Assets/model.off"
	 * @param setCenter Set the center as the mean vertex, see ApplyScale
	 * @param normalizeScale Whether to normalize the y-scale to 1
	 * @param scale Scale the mesh by this factor *after normalization*
	 * @param VPtr Pointer to the first element of the Vertex matrix
	 * @param VSize Number of vertices, rows of V
	 * @param NPtr Pointer to the first element of the Normals matrix
	 * @param NSize Number of normals, usually equal to VSize
	 * @param calculateNormalsIfEmpty Calculate per vertex normals, if no normals are present in the .off file
	 * @param FPtr Pointer to the first element of the Face/Indices matrix, one row is a triangle
	 * @param FSize Number of faces, rows of F
	 */
	UNITY_INTERFACE_EXPORT void
	ReadOFF(const char* path, const bool setCenter, const bool normalizeScale, const float scale, void*& VPtr,
	        int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize, bool calculateNormalsIfEmpty = false);

	// --- ModifyMesh.cpp
	UNITY_INTERFACE_EXPORT void TranslateMesh(State* state, Vector3 value);
	UNITY_INTERFACE_EXPORT void TranslateSelection(State* state, Vector3 value, int selectionId = -1);
	UNITY_INTERFACE_EXPORT void
	TransformSelection(State* state, int selectionId, Vector3 translation, float scale, Quaternion rotation);
	UNITY_INTERFACE_EXPORT void
	Harmonic(State* state, unsigned int boundaryMask = -1, bool showDeformationField = true);
	UNITY_INTERFACE_EXPORT void Arap(State* state, unsigned int boundaryMask = -1);
	UNITY_INTERFACE_EXPORT void ResetV(State* state);

	// --- Selection.cpp
	UNITY_INTERFACE_EXPORT void SelectSphere(State* state, Vector3 position, float radius, int selectionId = 0,
	                                         unsigned int selectionMode = SelectionMode::Add);
	UNITY_INTERFACE_EXPORT unsigned int GetSelectionMaskSphere(State* state, Vector3 position, float radius);
	UNITY_INTERFACE_EXPORT void ClearSelection(State* state, int selectionId = -1);
	UNITY_INTERFACE_EXPORT void ClearSelectionMask(State* state, unsigned int maskId);
	UNITY_INTERFACE_EXPORT void SetColorBySelection(State* state, int selectionId = -1);
	UNITY_INTERFACE_EXPORT void SetColorByMask(State* state, unsigned int maskId = -1);

	// --- sample/CustomUploadMesh.cpp
	// UNITY_INTERFACE_EXPORT UnityRenderingEventAndData GetUploadMeshPtr();
	// UNITY_INTERFACE_EXPORT void UploadMesh(int eventId, void* data);

	} // extern "C"
}