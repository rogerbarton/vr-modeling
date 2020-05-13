#include "InterfaceTypes.h"
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

	S->setZero();
}

Color_t Color::White ({1,1,1,1});
Color_t Color::Black ({0, 0, 0, 1});
Color_t Color::Red   ({0.7735849, 0.3280911, 0.280972, 1});
Color_t Color::Green ({0.4173074, 0.7264151, 0.366634, 1});
Color_t Color::Blue  ({0.3019607, 0.4429668, 0.858823, 1});
Color_t Color::Orange({0.8784314, 0.5314119, 0.145098, 1});

const Color_t& Color::GetColorById(int id) {
	return *(&Red + id % 4);
}
