# include "InterfaceTypes.h"

State::State(MeshDataNative& udata) {
	auto S = new Eigen::VectorXi(udata.VSize);
	SPtr = S->data();
}