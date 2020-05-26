#include "Selection.h"
#include "Interface.h"
#include <igl/slice.h>

void SphereSelect(State* state, Vector3 position, float radiusSqr, int selectionId, unsigned int selectionMode) {
	const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);
	const unsigned int maskId = 1 << selectionId;

	using BinaryExpr = const std::function<int(int, int)>;
	BinaryExpr AddSelection = [&](int a, int s) -> int { return a << selectionId | s; };
	BinaryExpr SubtractSelection = [&](int a, int s) -> int { return ~(a << selectionId) & s; };
	BinaryExpr ToggleSelection = [&](int a, int s) -> int { return a << selectionId ^ s; };

	BinaryExpr* Apply;
	if(selectionMode == SelectionMode::Add)
		Apply = &AddSelection;
	else if(selectionMode == SelectionMode::Subtract)
		Apply = &SubtractSelection;
	else if(selectionMode == SelectionMode::Toggle)
		Apply = &ToggleSelection;
	else
	{
		LOGERR("Invalid selection mode: " << selectionMode);
		return;
	}

	*state->S = ((state->V->rowwise() - posMap).array().square().matrix().rowwise().sum().array() < radiusSqr)
			.cast<int>().matrix()
			.binaryExpr(*state->S, *Apply);

	// Get selection size
	state->SSize[selectionId] = state->S->unaryExpr([&](int a) -> int { return a & maskId; }).sum();
	state->SSizeAll = state->S->unaryExpr([&](int a) -> int { return a > 0; }).sum();
	// LOG("Selected: " << state->SSize[selectionId] << " vertices, total selected: " << state->SSizeAll);

	state->DirtySelections |= maskId;
}

/**
 * Resets a particular selection
 * @param selectionId  Which selection to clear, -1 to clear all selections
 */
void ClearSelection(State* state, int selectionId){
	ClearSelectionMask(state, Selection::GetMask(selectionId));
}

void ClearSelectionMask(State* state, unsigned int maskId) {
	*state->S = state->S->unaryExpr([&](int s) -> int { return ~maskId & s; });
	state->DirtySelections |= maskId;
}

/**
 * Set the vertex colors to show a single selection
 * @param selectionId Which selection to show
 */
void SetColorBySelection(State* state, int selectionId) {
	const unsigned int maskId = Selection::GetMask(selectionId);
	const auto& color = Color::GetColorById(selectionId);

	const auto mask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).cast<float>().eval();
	*state->C = mask * color + (1.f - mask.array()).matrix() * Color::White;

	state->DirtyState |= DirtyFlag::CDirty;
}

/**
 * Set the color based on a mask of visible selections
 * @param maskId Which selections to show
 */
void SetColorByMask(State* state, unsigned int maskId) {
	state->C->setZero();

	unsigned int selectionId = 0;
	while (selectionId < state->Input.SCount) {
		const unsigned int m = 1 << selectionId;
		if ((m & maskId) == 0) {// Skip selections that are not visible
			++selectionId;
			continue;
		}

		// Normalize color by the maskId so we can multiply the mask by the color
		const Eigen::MatrixXf mask = state->S->unaryExpr([&](int a) -> int { return a & m; }).cast<float>();

		const Color_t color = Color::GetColorById(selectionId).array() / (float) m;
		*state->C += mask * color;

		++selectionId;
	}

	state->DirtyState |= DirtyFlag::CDirty;
}

unsigned int Selection::GetMask(int selectionId) {
	return selectionId == -1 ? -1 : 1u << selectionId;
}
