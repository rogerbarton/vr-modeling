#include "InterfaceTypes.h"
#include <array>

Color_t Color::White ({1, 1, 1, 1});
Color_t Color::Black ({0, 0, 0, 1});
Color_t Color::Red   ({0.7735849f, 0.3280911f, 0.280972f, 1});
Color_t Color::Green ({0.4173074f, 0.7264151f, 0.366634f, 1});
Color_t Color::Blue  ({0.3019607f, 0.4429668f, 0.858823f, 1});
Color_t Color::Orange({0.8784314f, 0.5314119f, 0.145098f, 1});

const Color_t& Color::GetColorById(int selectionId) {
	const std::array<Color_t*, 4> colors = {&Red, &Green, &Blue, &Orange};
	return *colors[selectionId % colors.size()];
}

Eigen::Vector3f Vector3::AsEigen() const {
	Eigen::Vector3f v;
	v << x,y,z;
	return v;
}

Eigen::RowVector3f Vector3::AsEigenRow() const {
	Eigen::RowVector3f v;
	v << x,y,z;
	return v;
}
