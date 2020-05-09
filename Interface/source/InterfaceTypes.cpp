#include "InterfaceTypes.h"

State::State(const UMeshDataNative udata) {
	S = new Eigen::VectorXi(udata.VSize);
}