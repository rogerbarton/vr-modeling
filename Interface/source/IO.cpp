#include "IO.h"
#include "Interface.h"
#include "Util.h"
#include <igl/readOFF.h>

namespace Interface
{
	void ApplyDirty(State* state, const UMeshDataNative data, const unsigned int visibleSelectionMask)
	{
		auto& dirty = state->DirtyState;

		if (state->DirtySelections > 0)
		{
			// Update selection sizes
			state->SSizeAll = state->S->unaryExpr([&](int a) -> int { return a > 0; }).sum();

			for (unsigned int selectionId = 0; selectionId < state->SCount; ++selectionId)
			{
				const unsigned int maskId = 1u << selectionId;
				if ((maskId & state->DirtySelections) == 0)
					continue;

				auto before = state->SSize[selectionId];
				state->SSize[selectionId] = state->S->unaryExpr([&](int a) -> int { return (a & maskId) > 0; }).sum();

				// Set flag if size has changed
				if (before != state->SSize[selectionId])
					state->DirtySelectionsResized |= 1 << selectionId;
			}
			state->Native->DirtySelectionsForBoundary |= state->DirtySelectionsResized;

			// Set Colors if a visible selection is dirty
			if ((state->DirtySelections & visibleSelectionMask) > 0 &&
			    (dirty & DirtyFlag::DontComputeColorsBySelection) == 0)
				SetColorByMask(state, visibleSelectionMask);

		}

		if ((dirty & DirtyFlag::VDirty) > 0)
		{
			TransposeToMap(state->V, data.VPtr);
			state->Native->DirtyBoundaryConditions = true;
		}
		if ((dirty & DirtyFlag::NDirty) > 0)
			TransposeToMap(state->N, data.NPtr);
		if ((dirty & DirtyFlag::CDirty) > 0)
			TransposeToMap(state->C, data.CPtr);
		if ((dirty & DirtyFlag::UVDirty) > 0)
			TransposeToMap(state->UV, data.UVPtr);
		if ((dirty & DirtyFlag::FDirty) > 0)
			TransposeToMap(state->F, data.FPtr);
	}

	void LoadOFF(const char* path, const bool setCenter, const bool normalizeScale, const float scale,
	             void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize)
	{
		auto* V = new V_RowMajor_t(); //Must use new as we delete in C#
		auto* N = new V_RowMajor_t();
		auto* F = new F_RowMajor_t();

		bool success = igl::readOFF(path, *V, *F, *N);

		//if (N->rows() == 0) //Calculate normals if they are not present
		//igl::per_vertex_normals(*V, *F, *N);

		VSize = V->rows();
		FSize = F->rows();
		NSize = N->rows();
		VPtr = V->data();
		FPtr = F->data();
		NPtr = N->data();

		ApplyScale<V_RowMajor_t>((float*) VPtr, VSize, setCenter, normalizeScale, scale);

		LOG("OFF Import " << (success ? "Successful: " : "Unsuccessful: ") << path)
	}
}