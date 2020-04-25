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
        V_t V = Eigen::Map<V_t>(VPtr, VSize, 3);
        const F_t F = Eigen::Map<F_t>(FPtr, FSize, 3);
        SparseV_t Q(VSize, 3);

        igl::harmonic(V, F, 2, Q);
        //V += Q;
    }
}