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

        LOG("Initialized Native.")
    }

    /**
     * Called when a new mesh is loaded. Initialize global variables, do pre-calculations for a mesh
     * @param data The unity MeshData
     * @param name Name of the mesh
     * @return A pointer to the C++ state for this mesh
     */
    State* InitializeMesh(const MeshDataNative data, const char* name)
	{
		// TODO: Pre-compute here
		LOG("InitializeMesh(): " << name)

		return new State(data);
	}

	/**
	 * Dispose all C++ state tied to a mesh properly
	 */
	void DisposeMesh(State* state){
		delete state;
	}

	/**
	 * Called when the plugin is loaded, this can be after/before Initialize()<p>
	 * Declared in IUnityInterface.h
	 * @param unityInterfaces Unity class for accessing minimal Unity functionality exposed to C++ Plugins
	 */
	void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
		s_UnityInterfaces = unityInterfaces;
		LOG("UnityPluginLoad()")
	}


	/**
	 * Called when the plugin is unloaded, clean up here
	 * Declared in IUnityInterface.h
	 */
	void UNITY_INTERFACE_API UnityPluginUnload()
	{
		s_UnityInterfaces = nullptr;

		LOG("UnityPluginUnload()")
		DebugLog = nullptr;
	}
}
