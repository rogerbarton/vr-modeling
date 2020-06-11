#pragma once

#include "NumericTypes.h"

/**
 * Contains all variables that are only used in C++ for a mesh
 */
struct NativeState{
	Eigen::MatrixXf* V0;
	//TODO: ARAP precomputation

	NativeState(Eigen::MatrixXf* V) : V0(new Eigen::MatrixXf(*V)) {}
};