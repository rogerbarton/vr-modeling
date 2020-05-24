#include "Interface.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->V;
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
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
