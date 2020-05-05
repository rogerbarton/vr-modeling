#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>
#include "InterfaceTypes.h"
#include <Eigen/core>

// Declare variables made extern in Interface.h
/**
 * Print to the Unity Debug.Log. Check that the function pointer is not null before using
 * @example @code if (DebugLog) DebugLog("Hello");
 */
StringCallback DebugLog = nullptr;

IUnityInterfaces* s_UnityInterfaces = nullptr;

extern "C" {
	/**
	 * Called just before the first function is called from this library.
	 * Use this to pass initial data from C#
	 * @see C# Native.Initialize()
	 * @param debugCallback A C# delegate (function pointer) to print to the Unity Debug.Log
	 */
	void Initialize(const StringCallback debugCallback) {
#ifndef NDEBUG
        DebugLog = debugCallback;
#endif

		Eigen::initParallel();
		Eigen::setNbThreads(std::max(1,Eigen::nbThreads() - 2)); //remove main and render thread

        if (DebugLog) DebugLog("Initialized Native.");
    }

    /**
     * Called when a new mesh is loaded. Initialize global variables, do pre-calculations for a mesh
     * @param name Name of the mesh
     * @param data Pointers to the MeshData
     * @param dirtyState Used to state which parts of the MeshData have been modified
     */
	void InitializeMesh(const char* name, const MeshDataNative data, unsigned int& dirtyState)
	{
		// TODO: Pre-compute here
		if (DebugLog) DebugLog((std::string("InitializeMesh(): ") + std::string(name)).data());
	}

	/**
	 * Called when the plugin is loaded, this can be after/before Initialize()<p>
	 * Declared in IUnityInterface.h
	 * @param unityInterfaces Unity class for accessing minimal Unity functionality exposed to C++ Plugins
	 */
	void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
		s_UnityInterfaces = unityInterfaces;
		if (DebugLog) DebugLog("UnityPluginLoad()");
	}


	/**
	 * Called when the plugin is unloaded, clean up here
	 * Declared in IUnityInterface.h
	 */
	void UNITY_INTERFACE_API UnityPluginUnload()
	{
		s_UnityInterfaces = nullptr;

		if (DebugLog) DebugLog("UnityPluginUnload()");
		DebugLog = nullptr;
	}
}
