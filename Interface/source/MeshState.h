#pragma once
#include "InterfaceTypes.h"
#include "MeshStateNative.h"
#include <Eigen/Core>

/**
 * Stores all data related to a specific mesh.
 * Members are shared between native(C++) and managed(C#).
 * Some members point to native only or managed only and are depicted as void*
 */
struct MeshState
{
	/**
	 * Tells us what has changed with the mesh using the DirtyFlag constants
	 */
	unsigned int DirtyState{DirtyFlag::None};
	/**
	 * Tells us which selections have been modified, as a bitmask. Each bit represents one selection.
	 */
	unsigned int DirtySelections{0};
	/**
	 * Less stricter version than DirtySelections,
	 * where we only consider a selection dirty if the selected vertices size changes, see SSizes.
	 */
	unsigned int DirtySelectionsResized{0};

	/**
	 * The vertex matrix in column major with dimensions VSize x 3.
	 * Stores position for each vertex, one row represents one vertex.
	 */
	Eigen::MatrixXf* V;
	/**
	 * The normals matrix in column major with dimensions VSize x 3.
	 * Stores the normal for each vertex.
	 */
	Eigen::MatrixXf* N;
	/**
	 * The rgba color matrix in column major with dimensions VSize x 4.
	 * Stores the color for each vertex.
	 */
	Eigen::MatrixXf* C;
	/**
	 * The UV0 matrix in column major with dimensions VSize x 2.
	 * Stores the 2D uv coordinate for each vertex.
	 */
	Eigen::MatrixXf* UV;
	/**
	 * The Face/Indices matrix in column major with dimensions FSize x 3.
	 * Stores the vertex indices for each face/triangle, one row represents one face.
	 */
	Eigen::MatrixXi* F;

	/**
	 * Number of vertices, columns in V
	 */
	int VSize{0};
	/**
	 * Number of faces, columns in F
	 */
	int FSize{0};

	/**
	 * The selection vector, stores the selection state for each vertex.
	 * We store one uint per vertex. The selection is represented as a bitmask, with each bit indicating if the
	 * vertex is in that selection or not. So there are max 32 selections (32 bits)
	 */
	Eigen::VectorXi* S;
	/**
	 * Amount of selections that are in use
	 */
	unsigned int SSize{1};
	/**
	 * uint[32], number of vertices selected per selection.
	 * Stored as a pointer so we can easily share this with C#.
	 */
	unsigned int* SSizes;
	/**
	 * Total vertices selected
	 */
	unsigned int SSizesAll{0};

	/**
	 * Native only state, a void* in C#
	 */
	MeshStateNative* Native;

	/**
	 * Initialise the shared state from a Unity mesh
	 * @param udata All data required to create the state
	 */
	explicit MeshState(UMeshDataNative udata);

	/**
	 * This is where all C++ allocated memory for a mesh is deleted.
	 */
	~MeshState();
};
