#define DllImport __declspec( dllimport )
#define DllExport __declspec( dllexport )

extern "C"{
    DllExport int IncrementValue(int value);
}