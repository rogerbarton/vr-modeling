#include "InterfaceTypes.h"

State::State(const MeshDataNative udata) {
	S = new Eigen::VectorXi(udata.VSize);
}