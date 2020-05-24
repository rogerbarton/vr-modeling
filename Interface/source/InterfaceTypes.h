#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <Eigen/Core>

// Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);

// Macro to easily concat strings using stringstream, use the operator<<
#define STR(message) static_cast<std::ostringstream &&>((std::ostringstream() << message)).str().data()
// Macro to easily print to the Unity Debug.Log
#ifndef NDEBUG
#define LOG(message) if(DebugLog) { DebugLog(STR(message)); }
#define LOGWARN(message) if(DebugLogWarning) { DebugLogWarning(STR(message)); }
#define LOGERR(message) if(DebugLogError) { DebugLogError(STR(message)); }
#else
#define LOG(m)
#endif

struct Vector3
{
	float x;
    float y;
    float z;

	Vector3() = default;
	Vector3(float x, float y, float z) : x(x), y(y), z(z) {}
	Vector3(Eigen::Vector3f value) : x(value(0)), y(value(1)), z(value(2)) {}
};

/**
 * Marks which data has changed in <code>UMeshDataNative</code> as a bitmask
 */
struct DirtyFlag {
	static const unsigned int None = 0;
	static const unsigned int All = 0xFFFFFFFF;
	static const unsigned int VDirty = 1;
	static const unsigned int NDirty = 2;
	static const unsigned int CDirty = 4;
	static const unsigned int UVDirty = 8;
	static const unsigned int FDirty = 16;
	/**
	 * Don't recaluclate normals when VDirty is set, <see cref="NDirty"/> overrides this.
	 */
	static const unsigned int DontComputeNormals = 32;
	/**
	 * Don't recalculate bounds when VDirty is set. Bounds are used for occlusion culling.
	 */
	static const unsigned int DontComputeBounds = 64;
};

/**
 * Stores all pointers to the MeshData arrays.<p>
 * Usually this should be as a <code>const</code> parameter
 */
struct UMeshDataNative
{
	float* VPtr;
	float* NPtr;
	float* CPtr;
	float* UVPtr;
	int* FPtr;

	int VSize;
	int FSize;
};

struct InputState
{
	// Translate
	bool Translate;

	// Select
	bool Select;
	int SelectActiveId;
	Vector3 SelectPos;
	float SelectRadiusSqr;

	// Harmonic
	bool Harmonic;

	InputState() = default;
};

struct State {
	unsigned int DirtyState = DirtyFlag::None;

	Eigen::MatrixXf* V;
	Eigen::MatrixXf* N;
	Eigen::MatrixXf* C;
	Eigen::MatrixXf* UV;
	Eigen::MatrixXi* F;
	
	int VSize = 0;
	int FSize = 0;

	// Latest InputState from PreExecute
	InputState Input;

	// Private C++ state
	Eigen::VectorXi* S;
	int SSize = 0;  // Amount of vertices selected in activeSelection
	int SCount = 0; // Number of selections

	explicit State(const UMeshDataNative udata);
};

using Color_t = Eigen::RowVector4f;
struct Color{
	static Color_t White;
	static Color_t Black;
	static Color_t Red;
	static Color_t Green;
	static Color_t Blue;
	static Color_t Orange;
	static const Color_t& GetColorById(int id);
};

struct SelectionMode{
	static const unsigned int Add = 0;
	static const unsigned int Subtract = 1;
	static const unsigned int Toggle = 2;
};