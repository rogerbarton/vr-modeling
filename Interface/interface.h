#define DllExport __declspec( dllexport )

#include <string>

//Function pointer to a C# void MyFct(string message)
typedef void(__stdcall* StringCallback) (const char* message);
StringCallback DebugLog;

typedef void(__stdcall* VFCallback) (float* V, int* F);
VFCallback CreateMesh;

struct Vector3
{
    float x;
    float y;
    float z;
};

extern "C"{
    //For passing an array of structs
    /*struct VertexPos{
        float pos[3];
    };

    struct Face {
        int tri[3];
    };*/

    DllExport void InitializeNative(const char* modelRootp, StringCallback debugCallback, VFCallback createMeshCallback);
    DllExport int IncrementValue(int value);
    DllExport void LoadMesh(const std::string modelPath);

    DllExport void FillMesh(float* V, int VSize, unsigned int* F, int FSize);
    DllExport void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr);
    DllExport void ComputeColors(float* outColors, float* Vptr, int nV);
}