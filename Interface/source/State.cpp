#include "State.h"
#include "IO.h"

State::State(const UMeshDataNative udata) {
	VSize = udata.VSize;
	FSize = udata.FSize;

	V = new Eigen::MatrixXf(VSize, 3);
	N = new Eigen::MatrixXf(VSize, 3);
	C = new Eigen::MatrixXf(VSize, 4);
	UV = new Eigen::MatrixXf(VSize, 2);
	F = new Eigen::MatrixXi(FSize, 3);

	S = new Eigen::VectorXi(VSize);

	// Copy over data
	TransposeFromMap(udata.VPtr, V);
	TransposeFromMap(udata.NPtr, N);
	TransposeFromMap(udata.CPtr, C);
	TransposeFromMap(udata.UVPtr, UV);
	TransposeFromMap(udata.FPtr, F);

	// Selection
	S->setZero();
	DirtySelections = (unsigned int)-1;

	SSize = new unsigned int[32];
	std::fill(SSize, SSize + 32, 0);

	Native = new NativeState{};
}

State::~State() {
	delete V;
	delete N;
	delete C;
	delete UV;
	delete F;
	delete S;
	delete[] SSize;
	delete Native;
}
