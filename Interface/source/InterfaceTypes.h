#pragma once
#include "NativeCallbacks.h"
#include <Eigen/Core>
#include <Eigen/Geometry>

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

	/**
	 * @warning Eigen has a different ordering of the values, handled safely by this function.
	 * We cannot simply reinterpret the bits.
	 */
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
	 * Don't recaluclate normals when VDirty is set. NDirty overrides this.
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

	/**
	 * Use this when the vertex positions have changed, but the boundary conditions are unaffected.
     * VDirty overrides this.
	 */
	static const unsigned int VDirtyExclBoundary = 256;

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
