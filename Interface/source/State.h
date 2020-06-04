#pragma once
#include "InterfaceTypes.h"
#include <Eigen/Core>

struct InputState
{
	unsigned int ActiveTool{ToolType::Select};

	// Generic Input
	float GripL{0};
	float GripR{0};
	Vector3 HandPosL{Vector3::Zero()};
	Vector3 HandPosR{Vector3::Zero()};
	// The previous position of the hand when the last transformation was made
	Vector3 PrevTrafoHandPosL{Vector3::Zero()};
	Vector3 PrevTrafoHandPosR{Vector3::Zero()};

	// Transform
	bool DoTransform{false};
	bool PrimaryTransformHand{false}; // True=R
	bool SecondaryTransformHandActive{false};

	// Select
	int ActiveSelectionId{0};
	unsigned int ActiveSelectionMode{0};
	unsigned int SCount{0};
	unsigned int VisibleSelectionMask{(unsigned int)-1}; // All visible initially
    bool VisibleSelectionMaskChanged{false};

	bool DoSelect{false};
	Vector3 SelectPos{Vector3::Zero()};
	float SelectRadiusSqr{0.1f};
	// A Mask of the selections that should be cleared
	unsigned int DoClearSelection{0};

	// Deformations
	bool DoHarmonicOnce{false}; // Trigger execution once
	bool DoHarmonic{false};     // Trigger execution every frame
	bool HarmonicShowDisplacement{false};
	bool DoArapOnce{false};
	bool DoArap{false};
};

struct State {
	unsigned int DirtyState{DirtyFlag::None};
	unsigned int DirtySelections{0};

	Eigen::MatrixXf* V;
	Eigen::MatrixXf* N;
	Eigen::MatrixXf* C;
	Eigen::MatrixXf* UV;
	Eigen::MatrixXi* F;

	int VSize{0};
	int FSize{0};

	// Latest InputState from PreExecute
	InputState Input{};

	// Private C++ state
	Eigen::VectorXi* S;
	unsigned int* SSize; // uint[32], vertices selected per selection
	unsigned int SSizeAll{0}; // Total vertices selected

	explicit State(UMeshDataNative udata);

	~State();
};
