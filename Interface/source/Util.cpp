#include "Util.h"
#include <array>

Color_t Color::White({1, 1, 1, 1});
Color_t Color::Black({0, 0, 0, 1});
Color_t Color::Red({0.67f, 0.24f, 0.19f, 1});
Color_t Color::Green({0.5f, 0.71f, 0.29f, 1});
Color_t Color::Blue({0.23f, 0.38f, 0.85f, 1});
Color_t Color::Orange({0.87f, 0.51f, 0.16f, 1});
Color_t Color::Purple({0.65f, 0.34f, 0.62f, 1});
Color_t Color::GreenLight({0.16f, 0.62f, 0.41f, 1});
Color_t Color::BlueLight({0.17f, 0.62f, 0.71f, 1});
Color_t Color::Yellow({0.86f, 0.86f, 0.15f, 1});

const Color_t& Color::GetColorById(int selectionId)
{
	const std::array<Color_t*, 8> colors = {&Red, &Green, &Blue, &Orange, &Purple, &GreenLight, &BlueLight,
	                                        &Yellow};
	return *colors[selectionId % colors.size()];
}
