#include <IUnityInterface.h>
#include <string>

//Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);
StringCallback DebugLog;

extern "C" {
    //For passing an array of structs
    /*struct VertexPos{
        float pos[3];
    };

    struct Face {
        int tri[3];
    };*/

    UNITY_INTERFACE_EXPORT void Initialize(const char* modelRootp, StringCallback debugCallback);
}