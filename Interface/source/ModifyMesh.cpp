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

    void Harmonic(float* VPtr, int VSize, int* FPtr, int FSize) {
	    auto V = Eigen::Map<V_t>(VPtr, VSize, 3);
	    const auto F = Eigen::Map<F_t>(FPtr, FSize, 3);

	    Eigen::VectorXi b(1);
	    Eigen::MatrixXf D, D_bc(1, 3);
	    b << 0;
	    D_bc.setZero();

	    // TODO
//	    igl::harmonic(V, F, b, D_bc, 2.f, D);
//	    V = D;

	    if (DebugLog) DebugLog("Harmonic");
    }

    void SphereSelect(State* state, MeshDataNative& udata, Vector3 position, float radius) {
        auto V = Eigen::Map<V_t>(udata.VPtr, udata.VSize, 3);
        auto mask = Eigen::Map<Eigen::VectorXi>(state->SPtr, udata.VSize);
        const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);

        mask = ((V.rowwise() - posMap).array().square().matrix().colwise().sum().array() < radius).cast<int>();
    }
}
