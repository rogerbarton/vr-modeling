#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->VPtr;
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }

    void Harmonic(State* state) {
	    Eigen::VectorXi b(1);
	    Eigen::MatrixXf D, D_bc(1, 3);
	    b << 0;
	    D_bc.setZero();

	    LOG("Harmonic");

	    // TODO
	    // igl::harmonic(*state->VPtr, *state->FPtr, b, D_bc, 2.f, D);
	    // V = D;
    }

    void SphereSelect(State* state, Vector3 position, float radiusSqr) {
        auto& V = *state->VPtr;
        auto& mask = *state->S;
        const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);

	    mask = ((V.rowwise() - posMap).array().square().matrix().rowwise().sum().array() < radiusSqr).cast<int>();


	    LOG("Selected: " << mask.sum() << " vertices");
    }
}
