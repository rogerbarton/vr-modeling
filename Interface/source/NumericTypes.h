#pragma once
#include <Eigen/core>
#include <Eigen/SparseCore>

namespace Interface
{
	using V_RowMajor_t = Eigen::Matrix<float, Eigen::Dynamic, 3, Eigen::RowMajor>;
	using F_RowMajor_t = Eigen::Matrix<int, Eigen::Dynamic, 3, Eigen::RowMajor>;
}