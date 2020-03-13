#include <IUnityInterface.h>
#include <string>

typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);
StringCallback DebugLog;

extern "C"{
    UNITY_INTERFACE_EXPORT void InitializeNative(StringCallback debugCallback);
    UNITY_INTERFACE_EXPORT void LoadOFF(const char* path, const float scale, void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize);
}