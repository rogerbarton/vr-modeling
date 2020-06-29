#include "Interface.h"
#include "Util.h"

void SelectSphere(MeshState* state, Vector3 position, float radius, int selectionId, unsigned int selectionMode)
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

unsigned int GetSelectionMaskSphere(MeshState* state, Vector3 position, float radius)
{
	const Eigen::RowVector3f posEigen = position.AsEigenRow();
	const float radiusSqr = radius * radius;
	unsigned int mask = 0;

	mask = ((state->V->rowwise() - posEigen).array().square().matrix().rowwise().sum().array() < radiusSqr)
			 .cast<int>().matrix() // to VectorXi
			 .cwiseProduct(*state->S)
			// element = 0 or its mask if it is inside the sphere or not
			// We use a reduction to aggregate the mask
			// Sidenote: The lambda defines how to aggregate two integers and must be associative
			.redux([](const int a, const int b) -> int {
				auto c = a | b;
				return c;
			});

	return mask;
}

Vector3 GetSelectionCenter(MeshState* state, unsigned int maskId)
{
	using namespace Eigen;
	VectorXi rowMask;
	MatrixXf VSlice;
	igl::colon<int>(0, state->VSize, rowMask);
	rowMask.conservativeResize(
			std::stable_partition(rowMask.data(), rowMask.data() + state->VSize,
			                      [&](int i) -> bool { return (*state->S)(i) & maskId; })
			- rowMask.data());

	if(rowMask.rows() == 0)
		return Vector3::Zero();

	igl::slice(*state->V, rowMask, igl::colon<int>(0, 2), VSlice);

	Vector3f center = VSlice.colwise().mean();
	return (Vector3)center;
}

void ClearSelectionMask(MeshState* state, unsigned int maskId)
{
	*state->S = state->S->unaryExpr([&](int s) -> int { return ~maskId & s; });
	state->DirtySelections |= maskId;
}

void SetColorSingleByMask(MeshState* state, unsigned int maskId, int colorId)
{
	const auto& color = Color::GetColorById(colorId);

	const auto mask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).cast<float>().eval();
	*state->C = mask * color + (1.f - mask.array()).matrix() * Color::Gray;

	state->DirtyState |= DirtyFlag::CDirty;
}

void SetColorByMask(MeshState* state, unsigned int maskId)
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

	// Deselected Color
	const Eigen::MatrixXf deselectedMask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) == 0; }).cast<float>();
	*state->C += deselectedMask * Color::Gray;

	state->DirtyState |= DirtyFlag::CDirty;
}
