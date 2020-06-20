#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>

// Declare variables made extern in Interface.h
/**
 * Print to the Unity Debug.Log. Check that the function pointer is not null before using
 * @example @code if (DebugLog) DebugLog("Hello");
 */
StringCallback DebugLog = nullptr;
StringCallback DebugLogWarning = nullptr;
StringCallback DebugLogError = nullptr;

IUnityInterfaces* s_UnityInterfaces = nullptr;

extern "C" {
	void Initialize(const StringCallback debugCallback, StringCallback debugWarningCallback, StringCallback debugErrorCallback) {
#ifndef NDEBUG
        DebugLog = debugCallback;
        DebugLogWarning = debugWarningCallback;
        DebugLogError = debugErrorCallback;
#endif

		Eigen::initParallel();
		Eigen::setNbThreads(std::max(1,Eigen::nbThreads() - 2)); //remove main and render thread

        LOG("Initialized Native.")
    }

    State* InitializeMesh(const UMeshDataNative data, const char* name)
	{
		// LOG("InitializeMesh(): " << name)

		return new State(data);
	}

	void DisposeMesh(State* state){
		delete state;
	}

	void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
		s_UnityInterfaces = unityInterfaces;
		LOG("UnityPluginLoad()")
	}

	void UNITY_INTERFACE_API UnityPluginUnload()
	{
		s_UnityInterfaces = nullptr;

		LOG("UnityPluginUnload()")
		DebugLog = nullptr;
		DebugLogWarning = nullptr;
		DebugLogError = nullptr;
	}
}
