#pragma once

#include "NumericTypes.h"
#include<igl/arap.h>

/**
 * Contains all variables that are only used in C++ for a mesh
 */
struct NativeState{
	Eigen::VectorXi boundary;
	unsigned int DirtySelectionsForBoundary{0};
	unsigned int BoundaryMask{0};

	// Harmonic & ARAP
	// Initial V, before deformations
	Eigen::MatrixXf* V0;

	igl::ARAPData* arapData{nullptr};

	explicit NativeState(Eigen::MatrixXf* V) : V0(new Eigen::MatrixXf(*V)) {}

	virtual ~NativeState() {
		delete V0;
		delete arapData;
	}
};