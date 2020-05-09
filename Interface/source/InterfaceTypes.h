#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <Eigen/Core>

// Function pointer to a C# void MyFct(string message)
typedef void(UNITY_INTERFACE_API* StringCallback) (const char* message);

struct Vector3
{
	Vector3(float x, float y, float z) : x(x), y(y), z(z) {}
	Vector3(Eigen::Vector3f value) : x(value(0)), y(value(1)), z(value(2)) {}

	float x;
    float y;
    float z;
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

struct State {
	unsigned int DirtyState = DirtyFlag::None;

	Eigen::MatrixXf* VPtr = nullptr;
	Eigen::MatrixXf* NPtr = nullptr;
	Eigen::MatrixXf* CPtr = nullptr;
	Eigen::MatrixXf* UVPtr = nullptr;
	Eigen::MatrixXi* FPtr = nullptr;
	
	int VSize = 0;
	int FSize = 0;

	Eigen::VectorXi* S;

	explicit State(const UMeshDataNative udata);
};
