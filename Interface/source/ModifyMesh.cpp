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

    void Harmonic(State* state, int boundarySelectionId, bool showDeformationField) {
	    Eigen::MatrixXf D_bc;
	    const unsigned int maskId = Selection::GetMask(boundarySelectionId);

	    LOG("Harmonic");

	    // From Tutorial 401
	    // Create boundary selection
	    // TODO: Calculate this if the selection has changed since the last harmonic call
	    Eigen::VectorXi b;
	    igl::colon<int>(0, state->VSize, b);
	    b.conservativeResize(std::stable_partition(b.data(), b.data() + state->VSize,
	                                               [&](int i) -> bool { return (*state->S)(i) & maskId; }) - b.data());

	    // Create boundary conditions
	    igl::slice(*state->V, b, igl::colon<int>(0, 2), D_bc);

	    // Do Harmonic and apply it
	    if (showDeformationField) {
		    Eigen::MatrixXf V0_bc;
		    igl::slice(*state->Native->V0, b, igl::colon<int>(0, 2), V0_bc);
		    D_bc -= V0_bc;

		    igl::harmonic(*state->Native->V0, *state->F, b, D_bc, 2, *state->V);
		    *state->V += *state->Native->V0;
	    } else
		    igl::harmonic(*state->Native->V0, *state->F, b, D_bc, 2, *state->V);

	    state->DirtyState |= DirtyFlag::VDirty;
    }
}
