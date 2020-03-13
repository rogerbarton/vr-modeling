#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>
#include <IUnityGraphics.h>
#include "InterfaceTypes.h"
#include <Eigen/core>

std::string modelRoot = "";
extern "C" {
	// void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	// {
    //     //IUnityGraphics* graphics = unityInterfaces->Get<IUnityGraphics>();
	// }
    
	void Initialize(const char* modelRootp, const StringCallback debugCallback) {
        DebugLog = debugCallback;

        modelRoot = modelRootp;
        if (DebugLog) DebugLog((char*)(modelRoot + " used as modelRoot").data());

        if (DebugLog) DebugLog("Initialized Native.");
    }
}
