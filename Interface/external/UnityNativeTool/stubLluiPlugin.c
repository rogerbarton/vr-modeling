#include <stdio.h>
#include "IUnityInterface.h"

static IUnityInterfaces* s_IUnityInterfaces = NULL;

// Get the UnityInterfaces pointer as with a normal plugin, called by Unity
UNITY_INTERFACE_EXPORT void UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
	s_IUnityInterfaces = unityInterfaces;
}

/**
 * Allow us to retrieve this pointer from C#, so that we can manually call <code>UnityPluginLoad()</code> for mocked libraries
 */
UNITY_INTERFACE_EXPORT IUnityInterfaces* UNITY_INTERFACE_API GetUnityInterfacesPtr()
{
	return s_IUnityInterfaces;
}
