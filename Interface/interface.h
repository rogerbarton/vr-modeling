#define DllImport __declspec( dllimport )
#define DllExport __declspec( dllexport )

#include <string>

//Function pointer to a C# void MyFct(string message)
typedef void(__stdcall* StringCallback) (const char* message);
StringCallback DebugLog;

typedef void(__stdcall* VFCallback) (float* V, int* F);
VFCallback CreateMesh;


extern "C"{
    DllExport void InitializeNative(char const*const modelRootp, StringCallback debugCallback, VFCallback createMeshCallback);
    DllExport int IncrementValue(int value);
    void LoadMesh(std::string modelPath);
}