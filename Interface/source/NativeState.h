#pragma once

#include "NumericTypes.h"
#include<igl/arap.h>

/**
 * Contains all variables that are only used in C++ for a mesh
 */
struct NativeState {
	// --- Harmonic & ARAP

	// Vertex indices part of the boundary
	Eigen::VectorXi Boundary{Eigen::VectorXi::Zero(0)};
	// Positions of vertices in the Boundary (rows correspond)
	Eigen::MatrixXf BoundaryConditions{Eigen::VectorXf::Zero(0)};
	// The selections currently used for the Boundary
	
	unsigned int BoundaryMask{0};
	// Selections that have changed since the last time the Boundary was calculated
	unsigned int DirtySelectionsForBoundary{0};
	// Whether the boundary conditions have changed since the last time they were calculated
	bool DirtyBoundaryConditions{true};

	// Initial V, before deformations
	Eigen::MatrixXf* V0;

	// Pre-computations for Arap
	igl::ARAPData<float>* ArapData{nullptr};

	explicit NativeState(Eigen::MatrixXf* V) : V0(new Eigen::MatrixXf(*V)) {}

	virtual ~NativeState() {
		delete V0;
		delete ArapData;
	}

};