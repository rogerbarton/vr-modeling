#include <Eigen/core>
//Note: Data is in RowMajor
using V_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
using F_t = Eigen::Matrix<unsigned int, Eigen::Dynamic, 3, Eigen::RowMajor>;

struct Vector3
{
    float x;
    float y;
    float z;
};