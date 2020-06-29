#include "Deform.h"
#include <igl/harmonic.h>

// --- Deformations
bool UpdateBoundary(MeshState* state, unsigned int boundaryMask)
{
	if (state->Native->BoundaryMask == boundaryMask && (state->Native->DirtySelectionsForBoundary & boundaryMask) == 0)
		return false;

	igl::colon<int>(0, state->VSize, state->Native->Boundary);
	state->Native->Boundary.conservativeResize(
			std::stable_partition(state->Native->Boundary.data(), state->Native->Boundary.data() + state->VSize,
			                      [&](int i) -> bool { return (*state->S)(i) & boundaryMask; })
			- state->Native->Boundary.data());

	state->Native->BoundaryMask = boundaryMask;
	state->Native->DirtySelectionsForBoundary &= ~boundaryMask;
	state->Native->DirtyBoundaryConditions = true;
	return true;
}

bool UpdateBoundaryConditions(MeshState* state)
{
	if (!state->Native->DirtyBoundaryConditions)
		return false;

	igl::slice(*state->V, state->Native->Boundary, igl::colon<int>(0, 2), state->Native->BoundaryConditions);

	state->Native->DirtyBoundaryConditions = false;
	return true;
}

void Harmonic(MeshState* state, unsigned int boundaryMask, bool showDeformationField)
{

	// Create boundary conditions
	bool boundaryChanged = UpdateBoundary(state, boundaryMask);
	bool solveHarmonic = UpdateBoundaryConditions(state) || boundaryChanged;

	if (!solveHarmonic) return;

	// Do Harmonic and apply it
	if (showDeformationField)
	{
		Eigen::MatrixXf V0_bc;
		igl::slice(*state->Native->V0, state->Native->Boundary, igl::colon<int>(0, 2), V0_bc);

		igl::harmonic(*state->Native->V0, *state->F, state->Native->Boundary,
		              state->Native->BoundaryConditions - V0_bc, 2, *state->V);
		*state->V += *state->Native->V0;
	}
	else
		igl::harmonic(*state->Native->V0, *state->F, state->Native->Boundary, state->Native->BoundaryConditions, 2,
		              *state->V);

	state->DirtyState |= DirtyFlag::VDirtyExclBoundary;
}

void Arap(MeshState* state, unsigned int boundaryMask)
{

	bool recomputeArapData = UpdateBoundary(state, boundaryMask);
	bool solveArap = UpdateBoundaryConditions(state) || recomputeArapData;

	if (state->Native->ArapData == nullptr)
	{
		// Initialize
		state->Native->ArapData = new igl::ARAPData<float>();
		state->Native->ArapData->max_iter = 100;
		recomputeArapData = true;
		solveArap = true;
	}

	if (recomputeArapData)
	{
		LOG("Arap precompute...")
		igl::arap_precomputation(*state->Native->V0, *state->F, 3, state->Native->Boundary,
		                         *state->Native->ArapData);
		LOG("Arap precompute done.")
	}

	if (!solveArap) return;
    LOG("Arap solve...")

	igl::arap_solve(state->Native->BoundaryConditions, *state->Native->ArapData, *state->V);
	LOG("Arap solve done.")

	state->DirtyState |= DirtyFlag::VDirtyExclBoundary;
}
