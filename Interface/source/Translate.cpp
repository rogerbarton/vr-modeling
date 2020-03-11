#include "Translate.h"
#include <Eigen/core>

extern "C" {
    void TranslateMesh(float* VPtr, int VSize, Vector3 directionArr) {
        auto V = Eigen::Map<Eigen::MatrixXf>((float*)VPtr, VSize, 3);
        auto direction = Eigen::Map<Eigen::RowVector3f>(&directionArr.x);

        V.rowwise() += direction;
    }
}