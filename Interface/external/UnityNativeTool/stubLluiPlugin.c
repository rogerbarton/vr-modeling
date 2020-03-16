#include <stdio.h>
#include "IUnityInterface.h"

// A one file library that gets us the pointer to the UnityInterfaces
// This is used by the UnityNativeTool to call UnityPluginLoad and
// UnityPluginUnload for our mocked libraries. This library is not mocked.

static IUnityInterfaces* s_IUnityInterfaces = NULL;

// Get the UnityInterfaces pointer as with a normal plugin, called by Unity
UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_IUnityInterfaces = unityInterfaces;
}

// Allow us to retrieve this pointer from C#
UNITY_INTERFACE_EXPORT IUnityInterfaces* UNITY_INTERFACE_API GetUnityInterfacesPtr()
{
	return s_IUnityInterfaces;
}
