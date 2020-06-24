#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>

// Declare variables made extern in InterfaceTypes.h
StringCallback DebugLog = nullptr;
StringCallback DebugLogWarning = nullptr;
StringCallback DebugLogError = nullptr;

IUnityInterfaces* s_IUnityInterfaces = nullptr;

void Initialize(const StringCallback debugCallback, StringCallback debugWarningCallback,
                StringCallback debugErrorCallback)
{
#ifndef NDEBUG
	DebugLog = debugCallback;
	DebugLogWarning = debugWarningCallback;
	DebugLogError = debugErrorCallback;
#endif

	Eigen::initParallel();
	Eigen::setNbThreads(std::max(1, Eigen::nbThreads() - 2)); //remove main and render thread

	LOG("Initialized Native.")
}

State* InitializeMesh(const UMeshDataNative data, const char* name)
{
	// LOG("InitializeMesh(): " << name)
	auto* state = new State(data);

	// Reset color immediately
	SetColorByMask(state, 0);
	state->DirtyState |= DirtyFlag::CDirty;

	return state;
}

void DisposeMesh(State* state)
{
	delete state;
}

void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_IUnityInterfaces = unityInterfaces;
	LOG("UnityPluginLoad()")
}

void UNITY_INTERFACE_API UnityPluginUnload()
{
	s_IUnityInterfaces = nullptr;

	LOG("UnityPluginUnload()")
	DebugLog = nullptr;
	DebugLogWarning = nullptr;
	DebugLogError = nullptr;
}
