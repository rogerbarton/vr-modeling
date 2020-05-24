#include "Interface.h"
#include <igl/colon.h>
#include <igl/slice.h>

void SphereSelect(State* state, Vector3 position, float radiusSqr, int selectionId, int selectionMode) {
	const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);
	const int maskId = 1 << selectionId;
	state->SCount = std::max(state->SCount, selectionId + 1);

	using BinaryExpr = const std::function<int(int, int)>;
	BinaryExpr AddSelection = [&](int a, int s) -> int { return a << selectionId | s; };
	BinaryExpr SubtractSelection = [&](int a, int s) -> int { return ~(a << selectionId) & s; };
	BinaryExpr ToggleSelection = [&](int a, int s) -> int { return a << selectionId ^ s; };

	BinaryExpr* Apply;
	if(selectionMode == SelectionMode::Add)
		Apply = &AddSelection;
	else if(selectionMode == SelectionMode::Subtract)
		Apply = &SubtractSelection;
	else if(selectionMode == SelectionMode::Subtract)
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
	state->SSize = state->S->unaryExpr([&](int a) -> int { return a & maskId; }).sum();
	// LOG("Selected: " << state->SSize << " vertices");

	// Set Colors
	SetColorBySelection(state, selectionId);
}

/**
 * Resets a particular selection
 * @param selectionId  Which selection to clear, -1 to clear all selections
 */
void ClearSelection(State* state, int selectionId){

	const unsigned int maskId = selectionId == 0 ? 0 : ~(1u << (unsigned int)selectionId);
	state->S->unaryExpr([&](int& s)->int{
		return maskId & s;
	});
}

/**
 * Set the vertex colors based on the selection
 * @param selectionId Which selection to show, -1 to show all selections
 */
void SetColorBySelection(State* state, int selectionId) {

	if(selectionId >= 0)
	{
		const int maskId = 1 << selectionId;
		const auto& color = Color::GetColorById(selectionId);

		const auto mask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).cast<float>().eval();
		*state->C = mask * color + (1.f - mask.array()).matrix() * Color::White;
	}
	else
	{
		// Loop over for each selection and add the color, assumes there are not multiple selections per vertex
		// Normalize color by the maskId so we can save doing this computation for each element in S
		while(selectionId < state->SCount) {
			const int maskId = 1 << selectionId;

			Color_t color = Color::GetColorById(selectionId).array() / maskId;
			// copy from above
			const auto mask = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).cast<float>().eval();
			*state->C += mask * color;
			selectionId++;
		}
	}
	state->DirtyState |= DirtyFlag::CDirty;
}
