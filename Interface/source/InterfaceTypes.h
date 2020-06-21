#pragma once
#include <PluginAPI/IUnityInterface.h>
#include <Eigen/Core>
#include <Eigen/Geometry>
/** Function pointer to a C# delegate: <code>void MyFct(string message)</code> */
typedef void(UNITY_INTERFACE_API* StringCallback)(const char* message);

// A global variable should be extern, so it can be seen in several cpp's.
// It is then defined in the Interface.cpp once
/**
 * Print to the Unity Debug.Log. Check that the function pointer is not null before using
 * <example><code>if (DebugLog) DebugLog("Hello");</code></example>
 * This is what the LOG macro does, use that instead.
 */
extern StringCallback DebugLog;
extern StringCallback DebugLogWarning;
extern StringCallback DebugLogError;

extern IUnityInterfaces* s_IUnityInterfaces;

/**
 * Macro to easily concatenate strings using stringstream, use the operator&lt;&lt;<br/>
 * <example><code>STR("My value: " &lt;&lt; 123)</code></example>
 */
#define STR(message) static_cast<std::ostringstream &&>((std::ostringstream() << message)).str().data()
#ifndef NDEBUG
/**
 * Macro to easily and safely print to the Unity Debug.Log, disabled in release. Uses STR.<br/>
 * <example><code> LOG("My value: " &lt;&lt; 123)</code></example>
 */
#define LOG(message) if(DebugLog) { DebugLog(STR(message)); }
/**
 * Call Unity Debug.LogWarning safely. Uses STR.
 */
#define LOGWARN(message) if(DebugLogWarning) { DebugLogWarning(STR(message)); }
/**
 * Call Unity Debug.LogError safely. Uses STR.
 */
#define LOGERR(message) if(DebugLogError) { DebugLogError(STR(message)); }
#else
#define LOG(m)
#define LOGWARN(m)
#define LOGERR(m)
#endif

/**
 * The Unity Vector3 with functonality for converting to/from Eigen::Vector3f (float).
 */
struct Vector3
{
	float x;
	float y;
	float z;

	Vector3() = default;

	Vector3(float x, float y, float z) : x(x), y(y), z(z)
	{}

	explicit Vector3(Eigen::Vector3f value) : x(value(0)), y(value(1)), z(value(2))
	{}

	// Do not overload cast operator, this causes conflicts with Eigen's cast operator on some versions
	// explicit operator Eigen::RowVector3f() const;
	// explicit operator Eigen::Vector3f() const;
	Eigen::Vector3f AsEigen() const;

	Eigen::RowVector3f AsEigenRow() const;

	inline static Vector3 Zero()
	{ return Vector3(0.f, 0.f, 0.f); }
};

/**
 * The Unity Quaternion with functonality for converting to/from Eigen::Quaternionf (float).<p>
 * <b>Beware:</b> Unity and Eigen have different conventions for ordering the values.
 */
struct Quaternion
{
	float x;
	float y;
	float z;
	float w;

	Quaternion() = default;

	Quaternion(float x, float y, float z, float w) : x(x), y(y), z(z), w(w)
	{}

	explicit Quaternion(Eigen::Quaternionf& q) : x(q.x()), y(q.y()), z(q.z()), w(q.w())
	{}

	// Note! Eigen has a different ordering of the values.
	inline Eigen::Quaternionf AsEigen() const
	{ return Eigen::Quaternionf(w, x, y, z); }

	inline static Quaternion Identity()
	{ return Quaternion(0.f, 0.f, 0.f, 1.0f); }
};

/**
 * Marks which data has changed in <code>UMeshDataNative</code> as a bitmask
 */
struct DirtyFlag
{
	static const unsigned int None = 0;
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
	/**
	 * Don't recompute colors if a visible selection has changed.
	 */
	static const unsigned int DontComputeColorsBySelection = 128;

	static const unsigned int All =
			(unsigned int) -1 - DontComputeNormals - DontComputeBounds - DontComputeColorsBySelection;
};

/**
 * Stores all pointers to the MeshData arrays.<p>
 * Usually this should be as a <code>const</code> parameter.
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
 * Constants related to how a select operation modifies the current selection.
 */
struct SelectionMode
{
	static const unsigned int Add = 0;
	static const unsigned int Subtract = 1;
	static const unsigned int Toggle = 2;
};
