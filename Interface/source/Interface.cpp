#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>
#include "InterfaceTypes.h"
#include <Eigen/core>

// Declare variables made extern in Interface.h
StringCallback DebugLog = nullptr;

IUnityInterfaces* s_UnityInterfaces = nullptr;

extern "C" {
	// Called just before the first function is called from this library
	// Use this to pass initial data from C#, See C# Native.Initialize()
	void Initialize(const StringCallback debugCallback) {
#ifndef NDEBUG
        DebugLog = debugCallback;
#endif

		Eigen::initParallel();
		Eigen::setNbThreads(std::max(1,Eigen::nbThreads() - 2)); //remove main and render thread

        if (DebugLog) DebugLog("Initialized Native.");
    }

	// Called when the plugin is loaded, this can be after/before Initialize()
	void UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
		s_UnityInterfaces = unityInterfaces;
		if (DebugLog) DebugLog("UnityPluginLoad()");
	}

	// Called when the plugin is unloaded, clean up here
	void UnityPluginUnload()
	{
		s_UnityInterfaces = nullptr;

		if (DebugLog) DebugLog("UnityPluginUnload()");
		DebugLog = nullptr;
	}
}
