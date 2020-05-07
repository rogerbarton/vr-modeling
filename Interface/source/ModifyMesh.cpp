#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(float* VPtr, int VSize, Vector3 value) {
        auto V = Eigen::Map<V_t>(VPtr, VSize, 3);
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }

    void Harmonic(State* state, const MeshDataNative udata) {
	    auto V = Eigen::Map<V_t>(udata.VPtr, udata.VSize, 3);
	    const auto F = Eigen::Map<F_t>(udata.FPtr, udata.FSize, 3);

	    Eigen::VectorXi b(1);
	    Eigen::MatrixXf D, D_bc(1, 3);
	    b << 0;
	    D_bc.setZero();

	    LOG("Harmonic");

	    // TODO
	    // igl::harmonic(V, F, b, D_bc, 2.f, D);
	    // V = D;
    }

    void SphereSelect(State* state, const MeshDataNative udata, Vector3 position, float radius) {
        auto V = Eigen::Map<V_t>(udata.VPtr, udata.VSize, 3);
        auto mask = *state->SPtr;
        const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);

	    mask = ((V.rowwise() - posMap).array().square().matrix().rowwise().sum().array() < radius).cast<int>();

	    LOG("Selected: " << mask.sum() << " vertices");
    }
}
