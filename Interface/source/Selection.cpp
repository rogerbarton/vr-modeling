#include "Interface.h"
#include "Util.h"

void SelectSphere(State* state, Vector3 position, float radius, int selectionId, unsigned int selectionMode)
{
	const Eigen::RowVector3f posEigen = position.AsEigenRow();
	const unsigned int maskId = 1 << selectionId;
	const float radiusSqr = radius * radius;

	using BinaryExpr = const std::function<int(int, int)>;
	BinaryExpr AddSelection = [&](int a, int s) -> int { return a << selectionId | s; };
	BinaryExpr SubtractSelection = [&](int a, int s) -> int { return ~(a << selectionId) & s; };
	BinaryExpr ToggleSelection = [&](int a, int s) -> int { return a << selectionId ^ s; };

	BinaryExpr* Apply;
	if (selectionMode == SelectionMode::Add)
		Apply = &AddSelection;
	else if (selectionMode == SelectionMode::Subtract)
		Apply = &SubtractSelection;
	else if (selectionMode == SelectionMode::Toggle)
		Apply = &ToggleSelection;
	else
	{
		LOGERR("Invalid selection mode: " << selectionMode);
		return;
	}

	*state->S = ((state->V->rowwise() - posEigen).array().square().matrix().rowwise().sum().array() < radiusSqr)
			.cast<int>().matrix() // to VectorXi
					// element = 0 or 1, if it is inside the sphere or not
			.binaryExpr(*state->S, *Apply);

	// LOG("Selected: " << state->SSize[selectionId] << " vertices, total selected: " << state->SSizeAll);

	state->DirtySelections |= maskId;
}

unsigned int GetSelectionMaskSphere(State* state, Vector3 position, float radius)
{
	const Eigen::RowVector3f posEigen = position.AsEigenRow();
	const float radiusSqr = radius * radius;
	unsigned int mask = 0;

	(((state->V->rowwise() - posEigen).array().square().matrix().rowwise().sum().array() < radiusSqr)
			 .cast<int>().matrix() // to VectorXi
	 * *state->S)
			// element = 0 or its mask if it is inside the sphere or not
			// For each vertex we call this lambda to aggregate the mask
			.unaryExpr([&mask](const int a) -> int {
				mask |= a;
				return a;
			});

	return mask;
}

void ClearSelectionMask(State* state, unsigned int maskId)
{
	*state->S = state->S->unaryExpr([&](int s) -> int { return ~maskId & s; });
	state->DirtySelections |= maskId;
}

void SetColorSingleByMask(State* state, unsigned int maskId, int colorId)
{
	const auto& color = Color::GetColorById(colorId);

	const auto mask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).cast<float>().eval();
	*state->C = mask * color + (1.f - mask.array()).matrix() * Color::White;

	state->DirtyState |= DirtyFlag::CDirty;
}

void SetColorByMask(State* state, unsigned int maskId)
{
	state->C->setZero();

	for (unsigned int selectionId = 0; selectionId < state->SCount; ++selectionId)
	{
		const unsigned int m = 1u << selectionId;
		if ((m & maskId) == 0) // Skip selections that are not visible
			continue;

		// Normalize color by the maskId so we can multiply the mask by the color
		const Eigen::MatrixXf mask = state->S->unaryExpr([&](int a) -> int { return a & m; }).cast<float>();

		const Color_t color = Color::GetColorById(selectionId).array() / (float) m;
		*state->C += mask * color;
	}

	state->DirtyState |= DirtyFlag::CDirty;
}
