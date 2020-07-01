#pragma once

#include<igl/arap.h>

/**
 * Contains all variables that are only used in C++ for a specific mesh.
 * We use one MeshStateNative per mesh.
 */
struct MeshStateNative
{
	// --- Harmonic & ARAP
	/**
	 * Vertices part of the boundary. Has a variable length.
	 * @note Evaluated in a lazy manner.
	 */
	Eigen::VectorXi Boundary{Eigen::VectorXi::Zero(0)};
	/**
	 * Positions of vertices in the Boundary (rows correspond). Has a variable length, the same as Boundary.
	 * @note Evaluated in a lazy manner.
	 */
	Eigen::MatrixXf BoundaryConditions{Eigen::VectorXf::Zero(0)};

	/**
	 * The selections currently used for the Boundary.
	 * @note Evaluated in a lazy manner.
	 */
	unsigned int BoundaryMask{0};
	/**
	 * Selections that have changed since the last time the Boundary was calculated.
	 * Used for lazy recalculation of Boundary.
	 */
	unsigned int DirtySelectionsForBoundary{0};
	/**
	 * Whether the boundary conditions have changed since the last time they were calculated.
	 * Used for lazy recalculation of BoundaryConditions.
	 */
	bool DirtyBoundaryConditions{true};

	/** Initial V, before deformations. Used for deformations and resetting V */
	Eigen::MatrixXf* V0;

	/**	The harmonic deformation field value at the last recalculation */
	bool harmonicShowDeformationField{false};

	/** Pre-computations for Arap */
	igl::ARAPData<float>* ArapData{nullptr};

	explicit MeshStateNative(Eigen::MatrixXf* V) : V0(new Eigen::MatrixXf(*V))
	{}

	virtual ~MeshStateNative()
	{
		delete V0;
		delete ArapData;
	}
};
