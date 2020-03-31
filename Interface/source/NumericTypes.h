#pragma once
#include <Eigen/core>
#include <Eigen/SparseCore>

using V_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::ColMajor>;
using F_t = Eigen::Matrix<int, Eigen::Dynamic, 3, Eigen::ColMajor>;
using SparseV_t = Eigen::SparseMatrix<float>;