#pragma once
#include <Eigen/core>
#include <Eigen/SparseCore>

using V_RowMajor_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
using F_RowMajor_t = Eigen::Matrix<int, Eigen::Dynamic, 3, Eigen::RowMajor>;
using SparseV_RowMajor_t = Eigen::SparseMatrix<float, Eigen::RowMajor, int>;

using V_t = Eigen::Matrix<float, Eigen::Dynamic, 3>;
using F_t = Eigen::Matrix<int, Eigen::Dynamic, 3>;
using SparseV_t = Eigen::SparseMatrix<float>;