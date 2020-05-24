#pragma once
#include "InterfaceTypes.h"
#include <Eigen/Core>

struct InputState
{
	unsigned int ActiveTool;

	// Generic Input
	float GripL;
	float GripR;
	Vector3 HandPosL;
	Vector3 HandPosR;
	// The previous position of the hand when the last transformation was made
	Vector3 PrevTrafoHandPosL;
	Vector3 PrevTrafoHandPosR;

	// Transform
	bool DoTransform;
	bool PrimaryTransformHand; // True=R
	bool SecondaryTransformHandActive;

	// Select
	int ActiveSelectionId;
	unsigned int ActiveSelectionMode;
	int SCount;

	bool DoSelect;
	Vector3 SelectPos;
	float SelectRadiusSqr;
	// A Mask of the selections that should be cleared
	int DoClearSelection;

	// Harmonic
	bool DoHarmonic;

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
	unsigned int* SSize; // uint[32], vertices selected per selection
	unsigned int SSizeAll; // Total vertices selected

	explicit State(UMeshDataNative udata);

	~State();
};
