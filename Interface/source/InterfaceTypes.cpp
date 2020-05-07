#include "InterfaceTypes.h"

State::State(const MeshDataNative udata) {
	SPtr = new Eigen::VectorXi(udata.VSize);
}