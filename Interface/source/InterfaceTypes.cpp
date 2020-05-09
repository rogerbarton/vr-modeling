#include "InterfaceTypes.h"
#include "IO.h"

State::State(const UMeshDataNative udata) {
	VSize = udata.VSize;
	FSize = udata.FSize;

	VPtr = new Eigen::MatrixXf(VSize, 3);
	NPtr = new Eigen::MatrixXf(VSize, 3);
	CPtr = new Eigen::MatrixXf(VSize, 4);
	UVPtr = new Eigen::MatrixXf(VSize, 2);
	FPtr = new Eigen::MatrixXi(FSize, 3);

	S = new Eigen::VectorXi(VSize);

	// Copy over data
	TransposeFromMap(udata.VPtr, VPtr);
	TransposeFromMap(udata.NPtr, NPtr);
	TransposeFromMap(udata.CPtr, CPtr);
	TransposeFromMap(udata.UVPtr, UVPtr);
	TransposeFromMap(udata.FPtr, FPtr);

	S->setZero();
}