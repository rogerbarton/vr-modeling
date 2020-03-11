#include <IUnityInterface.h>
#include <string>

//Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);
StringCallback DebugLog;

typedef void(UNITY_INTERFACE_API* VFCallback) (float* V, int* F);
VFCallback CreateMesh;



extern "C"{
    //For passing an array of structs
    /*struct VertexPos{
        float pos[3];
    };

    struct Face {
        int tri[3];
    };*/

    UNITY_INTERFACE_EXPORT void InitializeNative(const char* modelRootp, StringCallback debugCallback, VFCallback createMeshCallback);
    UNITY_INTERFACE_EXPORT int IncrementValue(int value);
    UNITY_INTERFACE_EXPORT void LoadMesh(const std::string modelPath);

    UNITY_INTERFACE_EXPORT void FillMesh(float* V, int VSize, unsigned int* F, int FSize);
    UNITY_INTERFACE_EXPORT void ComputeColors(float* outColors, float* Vptr, int nV);
}