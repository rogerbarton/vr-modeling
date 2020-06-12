#include "Interface.h"
#include "NumericTypes.h"
#include "Selection.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->V;

        V.rowwise() += (Eigen::RowVector3f)value;
    }

	void TranslateSelection(State* state, Vector3 value, int selectionId) {
		auto& V = *state->V;
		const auto& S = *state->S;
		const unsigned int maskId = Selection::GetMask(selectionId);

		for (int i = 0; i < V.rows(); ++i) {
			if ((S(i) & maskId) > 0) {
				V.row(i) += (Eigen::RowVector3f)value;
			}
		}
	}

    /**
     * Transform the selected vertices in place (translate + scale + rotate)
     * @param selectionId Which selection to transform, -1 for all selections
     */
    void TransformSelection(State* state, int selectionId, Vector3 translation, float scale, float angle, Vector3 axis){
	    auto& V = *state->V;
	    const auto& S = *state->S;
	    const unsigned int maskId = Selection::GetMask(selectionId);

	    using namespace Eigen;
	    Transform<float, 3, Affine> transform = Translation3f((Vector3f)translation) * AngleAxisf(angle, (Vector3f)axis) * Scaling(scale);

	    for (int i = 0; i < V.rows(); ++i) {
		    if((S(i) & maskId) > 0) {
			    Vector3f v = V.row(i);
			    V.row(i) = transform * v;
		    }
	    }
    }

	/**
	 * Recalculates the boundary based on if the relevant selections have changed
	 * @return True if boundary has changed
	 */
	bool UpdateBoundary(State* state, unsigned int boundaryMask) {
		if (state->Native->BoundaryMask == boundaryMask && (state->Native->DirtySelectionsForBoundary & boundaryMask) == 0) return false;

		igl::colon<int>(0, state->VSize, state->Native->boundary);
		state->Native->boundary.conservativeResize(
				std::stable_partition(state->Native->boundary.data(), state->Native->boundary.data() + state->VSize,
				                      [&](int i) -> bool { return (*state->S)(i) & boundaryMask; })
				- state->Native->boundary.data());

		state->Native->DirtySelectionsForBoundary &= ~boundaryMask;
		return true;
	}

    // From Tutorial 401
    void Harmonic(State* state, unsigned int boundaryMask, bool showDeformationField) {
	    LOG("Harmonic");

		// Create boundary conditions
		UpdateBoundary(state, boundaryMask);
	    Eigen::MatrixXf bc;
	    igl::slice(*state->V, state->Native->boundary, igl::colon<int>(0, 2), bc);

	    // Do Harmonic and apply it
	    if (showDeformationField) {
		    Eigen::MatrixXf V0_bc;
		    igl::slice(*state->Native->V0, state->Native->boundary, igl::colon<int>(0, 2), V0_bc);
		    bc -= V0_bc;

		    igl::harmonic(*state->Native->V0, *state->F, state->Native->boundary, bc, 2, *state->V);
		    *state->V += *state->Native->V0;
	    } else
		    igl::harmonic(*state->Native->V0, *state->F, state->Native->boundary, bc, 2, *state->V);

	    state->DirtyState |= DirtyFlag::VDirty;
    }

    void Arap(State* state, unsigned int boundaryMask) {
	    LOG("Arap");

	    bool recomputeArapData = UpdateBoundary(state, boundaryMask);
	    Eigen::MatrixXf bc;
	    igl::slice(*state->V, state->Native->boundary, igl::colon<int>(0, 2), bc);

	    if(state->Native->arapData == nullptr) {
		    state->Native->arapData = new igl::ARAPData();
		    state->Native->arapData->max_iter = 100;
		    recomputeArapData = true;
	    }

	    // if (recomputeArapData)
		//     igl::arap_precomputation(*state->Native->V0, *state->F, 3, state->Native->boundary, *state->Native->arapData);

	    // igl::arap_solve(bc, *state->Native->arapData, *state->V);

	    state->DirtyState |= DirtyFlag::VDirty;
    }
}
