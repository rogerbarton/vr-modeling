#include "MeshState.h"
#include "Util.h"

MeshState::MeshState(const UMeshDataNative udata)
{
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
	DirtySelections = (unsigned int) -1;
	DirtySelectionsResized = (unsigned int) -1;

	SSizes = new unsigned int[32];
	std::fill(SSizes, SSizes + 32, 0);

	Native = new MeshStateNative(V);
}

MeshState::~MeshState()
{
	delete V;
	delete N;
	delete C;
	delete UV;
	delete F;
	delete S;
	delete[] SSizes;
	delete Native;
}
