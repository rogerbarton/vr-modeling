#include "Translate.h"
#include <Eigen/core>

extern "C" {
    void TranslateMesh(float* VPtr, int VSize, Vector3 value) {
        auto V = Eigen::Map<V_t>((float*)VPtr, VSize, 3);
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }
}