#include "IO.h"
#include "Interface.h"
#include "NumericTypes.h"
#include "Util.h"
#include <igl/readOFF.h>
#include <igl/per_vertex_normals.h>

extern "C" {
    void ApplyDirty(State* state, const UMeshDataNative data){
    	auto& dirty = state->DirtyState;

	    if((dirty & DirtyFlag::VDirty) > 0)
		    TransposeToMap(state->VPtr, data.VPtr);
	    if((dirty & DirtyFlag::NDirty) > 0)
		    TransposeToMap(state->NPtr, data.NPtr);
	    if((dirty & DirtyFlag::CDirty) > 0)
		    TransposeToMap(state->CPtr, data.CPtr);
	    if((dirty & DirtyFlag::UVDirty) > 0)
		    TransposeToMap(state->UVPtr, data.UVPtr);
	    if((dirty & DirtyFlag::FDirty) > 0)
		    TransposeToMap(state->FPtr, data.FPtr);
    }

    void LoadOFF(const char* path, const bool setCenter, const bool normalizeScale, const float scale,
    		void*& VPtr, int& VSize, void*& NPtr, int& NSize, void*& FPtr, int& FSize) {
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

	    ApplyScale<V_RowMajor_t>((float*)VPtr, VSize, setCenter, normalizeScale, scale);

	    LOG("OFF Import " << (success ? "Successful: " : "Unsuccessful: ") << path)
    }
}
