#pragma once
#include "InterfaceTypes.h"
#include "NativeState.h"
#include <Eigen/Core>

/**
 * Stores all data related to a mesh.
 * Members are shared between native(C++) and managed(C#).
 * Some members point to native only or managed only
 */
struct State {
	unsigned int DirtyState{DirtyFlag::None};
	unsigned int DirtySelections{0};
	unsigned int DirtySelectionsResized{0};

	Eigen::MatrixXf* V;
	Eigen::MatrixXf* N;
	Eigen::MatrixXf* C;
	Eigen::MatrixXf* UV;
	Eigen::MatrixXi* F;

	int VSize{0};
	int FSize{0};

	Eigen::VectorXi* S;
	unsigned int* SSize; // uint[32], vertices selected per selection
	unsigned int SSizeAll{0}; // Total vertices selected
	unsigned int SCount{1}; // Amount of selections

	// Native only state
	NativeState* Native;

	explicit State(UMeshDataNative udata);

	~State();
};
