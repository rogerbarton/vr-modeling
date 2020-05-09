#pragma once

template <typename Matrix, typename Scalar>
void TransposeTo(Matrix* from, Scalar* to) {
	auto toMap = Eigen::Map<Matrix>(to, from->cols(), from->rows());
	toMap = from->transpose();
}
