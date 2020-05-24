#include "Interface.h"
#include "NumericTypes.h"
#include "Selection.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->V;
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
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

    void Harmonic(State* state) {
	    Eigen::MatrixXf D, D_bc;

	    LOG("Harmonic");

	    // From Tutorial 401
	    // Create boundary selection
	    Eigen::VectorXi b;
	    igl::colon<int>(0, state->VSize, b);
	    b.conservativeResize(std::stable_partition(b.data(), b.data() + state->SSize,
	    		[&state](int i)->bool{ return (*state->S)(i) >= 0; }) - b.data());

	    // Create boundary conditions
	    igl::slice(*state->V, b, igl::colon<int>(0, 2), D_bc);
	    // D_bc.rowwise() += Eigen::RowVector3f::Constant(0.1f);

	    // Do Harmonic and apply it
	    igl::harmonic(*state->V, *state->F, b, D_bc, 2, D);
	    *state->V = D;

	    state->DirtyState |= DirtyFlag::VDirty;
    }
}
