#include "InterfaceTypes.h"

Eigen::Vector3f Vector3::AsEigen() const
{
	Eigen::Vector3f v;
	v << x, y, z;
	return v;
}

Eigen::RowVector3f Vector3::AsEigenRow() const
{
	Eigen::RowVector3f v;
	v << x, y, z;
	return v;
}
