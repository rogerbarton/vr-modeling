#define DllImport __declspec( dllimport )
#define DllExport __declspec( dllexport )

//Function pointer to a C# void MyFct(string message)
typedef void(__stdcall* DebugCallback) (const char* message);
DebugCallback DebugLog;


extern "C"{
    DllExport void InitializeNative(DebugCallback callback);
    DllExport int IncrementValue(int value);
}