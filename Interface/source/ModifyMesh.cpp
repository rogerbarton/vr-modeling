#include "Interface.h"
#include "NumericTypes.h"
#include "Selection.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
	    state->V->rowwise() += value.AsEigenRow();
	    state->DirtyState |= DirtyFlag::VDirty;
    }

	void TranslateSelection(State* state, Vector3 value, int selectionId) {
		auto& V = *state->V;
		const auto& S = *state->S;
		const unsigned int maskId = Selection::GetMask(selectionId);
		const Eigen::RowVector3f valueEigen = value.AsEigenRow();

		for (int i = 0; i < V.rows(); ++i) {
			if ((S(i) & maskId) > 0) {
				V.row(i) += valueEigen;
			}
		}

		state->DirtyState |= DirtyFlag::VDirty;
	}

    /**
     * Transform the selected vertices in place (translate + scale + rotate)
     * @param selectionId Which selection to transform, -1 for all selections
     */
    void TransformSelection(State* state, int selectionId, Vector3 translation, float scale, Quaternion rotation) {
	    auto& V = *state->V;
	    const auto& S = *state->S;
	    const unsigned int maskId = Selection::GetMask(selectionId);

	    using namespace Eigen;
	    Transform<float, 3, Affine> transform =
			    Translation3f(translation.AsEigen()) * rotation.AsEigen() * Scaling(scale);

	    for (int i = 0; i < V.rows(); ++i) {
		    if ((S(i) & maskId) > 0) {
			    Vector3f v = V.row(i);
			    V.row(i) = transform * v;
		    }
	    }

	    state->DirtyState |= DirtyFlag::VDirty;
    }

	/**
	 * Recalculates the boundary based on if the relevant selections have changed
	 * @return True if boundary has changed
	 */
	bool UpdateBoundary(State* state, unsigned int boundaryMask) {
		if (state->Native->BoundaryMask == boundaryMask &&
		    (state->Native->DirtySelectionsForBoundary & boundaryMask) == 0)
			return false;

		igl::colon<int>(0, state->VSize, state->Native->Boundary);
		state->Native->Boundary.conservativeResize(
				std::stable_partition(state->Native->Boundary.data(), state->Native->Boundary.data() + state->VSize,
				                      [&](int i) -> bool { return (*state->S)(i) & boundaryMask; })
				- state->Native->Boundary.data());

		state->Native->DirtySelectionsForBoundary &= ~boundaryMask;
		state->Native->DirtyBoundaryConditions = true;
		return true;
	}

	/**
	 * Recalculates the boundary conditions for Harmonic and Arap
	 * @return True if the boundary conditions have changed
	 */
	bool UpdateBoundaryConditions(State* state, unsigned int boundaryMask) {
		if (!state->Native->DirtyBoundaryConditions)
			return false;

		igl::slice(*state->V, state->Native->Boundary, igl::colon<int>(0, 2), state->Native->BoundaryConditions);

		state->Native->DirtyBoundaryConditions = false;
		return true;
	}

    // From Tutorial 401
    void Harmonic(State* state, unsigned int boundaryMask, bool showDeformationField) {

	    // Create boundary conditions
	    bool boundaryChanged = UpdateBoundary(state, boundaryMask);
	    bool solveHarmonic = boundaryChanged || UpdateBoundaryConditions(state, boundaryMask);

	    if(!solveHarmonic) return;

	    // Do Harmonic and apply it
	    if (showDeformationField) {
		    Eigen::MatrixXf V0_bc;
		    igl::slice(*state->Native->V0, state->Native->Boundary, igl::colon<int>(0, 2), V0_bc);

		    igl::harmonic(*state->Native->V0, *state->F, state->Native->Boundary, state->Native->BoundaryConditions - V0_bc, 2, *state->V);
		    *state->V += *state->Native->V0;
	    } else
		    igl::harmonic(*state->Native->V0, *state->F, state->Native->Boundary, state->Native->BoundaryConditions, 2, *state->V);

	    state->DirtyState |= DirtyFlag::VDirty;
    }

    void Arap(State* state, unsigned int boundaryMask) {

	    bool recomputeArapData = UpdateBoundary(state, boundaryMask);
	    bool solveArap = recomputeArapData || UpdateBoundaryConditions(state, boundaryMask);

	    if (state->Native->ArapData == nullptr) {
		    state->Native->ArapData = new igl::ARAPData<float>();
		    state->Native->ArapData->max_iter = 100;
		    recomputeArapData = true;
	    }

	    if (recomputeArapData) {
	    	LOG("Arap precompute...")
		    igl::arap_precomputation(*state->Native->V0, *state->F, 3, state->Native->Boundary,
		                             *state->Native->ArapData);
	    	LOG("Arap precompute done.")
	    }

	    if(!solveArap) return;

	    igl::arap_solve(state->Native->BoundaryConditions, *state->Native->ArapData, *state->V);

	    state->DirtyState |= DirtyFlag::VDirty;
    }

	void ResetV(State* state) {
		*state->V = *state->Native->V0;
	}

}
