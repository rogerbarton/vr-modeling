#pragma once
#include "InterfaceTypes.h"
#include <Eigen/Core>

struct InputState
{
	unsigned int ActiveTool;

	// Generic Input
	float TriggerL;
	float TriggerR;
	Vector3 HandPosL;
	Vector3 HandPosR;
	Vector3 PrevHandPosL;
	Vector3 PrevHandPosR;

	// Transform
	bool DoTransform;
	bool PrimaryTransformHand; // True=R
	bool SecondaryTransformHandActive;

	// Select
	int SelectActiveId;
	int SSize;

	bool DoSelect;
	Vector3 SelectPos;
	float SelectRadiusSqr;

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
	int SSize = 0;  // Amount of vertices selected in activeSelection
	int SCount = 0; // Number of selections

	explicit State(UMeshDataNative udata);
};
