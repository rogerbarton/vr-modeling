#include "Interface.h"
#include <igl/readOBJ.h>
#include <igl/jet.h>
#include <Unity/IUnityGraphics.h>
#include "InterfaceTypes.h"
#include <Eigen/core>

#include <Unity/PlatformBase.h>
#include <Unity/RenderAPI.h>

StringCallback DebugLog;
std::string modelRoot = "";

IUnityInterfaces* s_UnityInterfaces = nullptr;
IUnityGraphics* s_Graphics = nullptr;

RenderAPI* s_CurrentAPI = nullptr;
UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

extern "C" {
	void Initialize(const char* modelRootp, const StringCallback debugCallback) {
#ifndef NDEBUG
        DebugLog = debugCallback;
#endif
        modelRoot = modelRootp;
        if (DebugLog) DebugLog((char*)(modelRoot + " used as modelRoot").data());

		Eigen::initParallel();
		Eigen::setNbThreads(std::max(1,Eigen::nbThreads() - 2)); //remove main and render thread

        if (DebugLog) DebugLog("Initialized Native.");
    }

	void UnityPluginLoad(IUnityInterfaces* unityInterfaces)
	{
		s_UnityInterfaces = unityInterfaces;
		s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
		s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

#if SUPPORT_VULKAN
		if (s_Graphics->GetRenderer() == kUnityGfxRendererNull)
		{
			extern void RenderAPI_Vulkan_OnPluginLoad(IUnityInterfaces*);
			RenderAPI_Vulkan_OnPluginLoad(unityInterfaces);
		}
#endif // SUPPORT_VULKAN

		// Run OnGraphicsDeviceEvent(initialize) manually on plugin load
		OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
		if (DebugLog) DebugLog("UnityPluginLoad()");
	}

	void UnityPluginUnload()
	{
		s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
		if (DebugLog) DebugLog("UnityPluginUnload()");
	}


	void OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
	{
		// Create graphics API implementation upon initialization
		if (eventType == kUnityGfxDeviceEventInitialize)
		{
			assert(s_CurrentAPI == NULL);
			s_DeviceType = s_Graphics->GetRenderer();
			s_CurrentAPI = CreateRenderAPI(s_DeviceType);
		}

		// Let the implementation process the device related events
		if (s_CurrentAPI)
		{
			s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
		}

		// Cleanup graphics API implementation upon shutdown
		if (eventType == kUnityGfxDeviceEventShutdown)
		{
			delete s_CurrentAPI;
			s_CurrentAPI = NULL;
			s_DeviceType = kUnityGfxRendererNull;
		}
	}

	UnityRenderingEventAndData GetUploadMeshPtr() {
		return UploadMesh;
	}

	//Has to have a UnityRenderingEventAndData signaturem so we store the data as a struct
    void UploadMesh(int eventId, void* dataPtr) {
		if (!s_CurrentAPI) {
			if (DebugLog) DebugLog("UploadMesh: CurrentAPI has not been initialized and is null cannot upload to GPU.");
			return;
		}
		// if (DebugLog) DebugLog("UploadMesh: uploading...");
		VertexUploadData* data = reinterpret_cast<VertexUploadData*>(dataPtr);

		if (data != nullptr && data->changed) {
			size_t bufferSize;
			float* bufferMapPtr = reinterpret_cast<float*>(s_CurrentAPI->BeginModifyVertexBuffer(data->gfxVertexBufferPtr, &bufferSize));
			// assert(data->VSize * 3 * 4 == bufferSize); //VSize * 3 dimensional position * 4Bytes each
			if (bufferMapPtr != nullptr)
			{
				std::copy(data->VPtr, data->VPtr + 3*data->VSize, bufferMapPtr);
				//memcpy(bufferMapPtr, data->VPtr, 4 * 3 * data->VSize);

				data->changed = 0;
				s_CurrentAPI->EndModifyVertexBuffer(data->gfxVertexBufferPtr);
			}

			{ //debugging, map again to see if values are saved
				float* bufferMapPtr = reinterpret_cast<float*>(s_CurrentAPI->BeginModifyVertexBuffer(data->gfxVertexBufferPtr, &bufferSize));
				s_CurrentAPI->EndModifyVertexBuffer(data->gfxVertexBufferPtr);

			}
		}
    }
}
