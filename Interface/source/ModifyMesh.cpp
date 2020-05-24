#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>
#include <igl/colon.h>
#include <igl/slice.h>

extern "C" {
    void TranslateMesh(State* state, Vector3 value) {
        auto& V = *state->V;
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }

    void Harmonic(State* state) {
	    Eigen::MatrixXf D, D_bc;

	    LOG("Harmonic");

	    // From Tutorial 401
	    // Create boundary selection
	    Eigen::VectorXi b;
	    igl::colon<int>(0, state->VSize, b);
	    b.conservativeResize(std::stable_partition(b.data(), b.data() + state->SSize,
	    		[&state](int i)->bool{ return (*state->S)(i) >= 0; }) - b.data());

	    // Create boundary conditions
	    igl::slice(*state->V, b, igl::colon<int>(0, 2), D_bc);
	    // D_bc.rowwise() += Eigen::RowVector3f::Constant(0.1f);

	    // Do Harmonic and apply it
	    igl::harmonic(*state->V, *state->F, b, D_bc, 2.f, D);
	    *state->V = D;

	    state->DirtyState |= DirtyFlag::VDirty;
    }

    void SphereSelect(State* state, Vector3 position, float radiusSqr, int selectionId, int selectionMode) {
	    const auto posMap = Eigen::Map<Eigen::RowVector3f>(&position.x);
	    const int maskId = 1 << selectionId;
	    state->SCount = std::max(state->SCount, selectionId + 1);

	    const std::function<int(int, int)> AddSelection = [&](int a, int s) -> int { return a << selectionId | s; };
	    const std::function<int(int, int)> SubtractSelection = [&](int a, int s) -> int { return ~(a << selectionId) & s; };
	    const std::function<int(int, int)> ToggleSelection = [&](int a, int s) -> int { return a << selectionId ^ s; };

	    const std::function<int(int, int)>* Apply;
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
}
