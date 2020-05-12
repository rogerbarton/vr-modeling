#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>
#include <igl/colon.h>
#include <igl/slice.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->VPtr;
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }

    void Harmonic(State* state) {
	    Eigen::MatrixXf D, V_bc;

	    LOG("Harmonic");

	    // From Tutorial 401
	    // Create boundary selection
	    Eigen::VectorXi b;
	    igl::colon<int>(0, state->VSize, b);
	    b.conservativeResize(std::stable_partition(b.data(), b.data() + state->SSize,
	    		[&state](int i)->bool{ return (*state->S)(i) >= 0; }) - b.data());

	    // Create boundary conditions
	    igl::slice(*state->VPtr, b, igl::colon<int>(0, 2), V_bc);
	    V_bc.rowwise() += Eigen::RowVector3f::Constant(0.1f);

	    // Do Harmonic and apply it
	    igl::harmonic(*state->VPtr, *state->FPtr, b, V_bc, 2.f, D);
	    *state->VPtr += D;

	    state->DirtyState |= DirtyFlag::VDirty;
    }

    void SphereSelect(State* state, Vector3 position, float radiusSqr) {
        auto& V = *state->VPtr;
        auto& mask = *state->S;
        const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);

	    mask = ((V.rowwise() - posMap).array().square().matrix().rowwise().sum().array() < radiusSqr).cast<int>();

	    // Get selection size
	    state->SSize = mask.sum();
	    LOG("Selected: " << state->SSize << " vertices");

	    // Set Colors
	    Eigen::RowVector4f White, Red;
	    White << 1.f, 1.f, 1.f, 1.f;
	    Red << 1.f, 0.2f, 0.2f, 1.f;

	    *state->CPtr = mask.cast<float>() * Red + (1.f - mask.cast<float>().array()).matrix() * White;

	    state->DirtyState |= DirtyFlag::CDirty;
    }
}
