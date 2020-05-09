#pragma once

/**
 * Transpose an Eigen::Matrix to an Eigen::Map, given by the pointer to the first element
 * Dimensions are inferred from the Matrix
 * @tparam Matrix An Eigen Matrix
 * @tparam Scalar Type on one element
 */
template <typename Matrix, typename Scalar>
void TransposeToMap(Matrix* from, Scalar* to) {
	auto toMap = Eigen::Map<Matrix>(to, from->cols(), from->rows());
	toMap = from->transpose();
}

/**
 * Transpose an Eigen::Map to an Eigen::Matrix
 * @tparam Scalar Type on one element
 * @tparam Matrix An Eigen Matrix
 */
template <typename Scalar, typename Matrix>
void TransposeFromMap(Scalar* from, Matrix* to) {
	auto fromMap = Eigen::Map<Matrix>(from, to->cols(), to->rows());
	*to = fromMap.transpose();
}
