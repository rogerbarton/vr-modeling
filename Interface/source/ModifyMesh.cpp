#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"
#include <igl/harmonic.h>

extern "C" {
    void TranslateMesh(float* VPtr, int VSize, Vector3 value) {
        auto V = Eigen::Map<V_t>(VPtr, VSize, 3);
        const auto valueMap = Eigen::Map<Eigen::RowVector3f>(&value.x);

        V.rowwise() += valueMap;
    }

    /**
     * Set the center/origin of the mesh to be the mean vertex
     * @param VPtr RowMajor Data
     */
    void CenterToMean(float* VPtr, int VSize){
    	assert(VPtr != nullptr);

	    auto V = Eigen::Map<V_RowMajor_t>(VPtr, VSize, 3);
	    Eigen::RowVector3f mean = V.colwise().mean();
		V.rowwise() -= mean;
    }

	/**
	 * Applies the scale of a mesh to the vertices, i.e. cwise multiply.
	 * If <code>targetScale</code> is set to zero, the model scale is normalized so it has unit height.
	 * @param normalize If set to true the absolute y-height of the model will be <code>targetScale</code> otherwise only
	 * the targetScale factor is applied.
	 */
	void ApplyScale(float* VPtr, int VSize, bool normalize, float targetScale){ // TODO: move this so it is part of LoadOFF
		auto V = Eigen::Map<V_RowMajor_t>(VPtr, VSize, 3);

		if(normalize) // Normalize by y-scale
		{
//			CenterToMean(VPtr, VSize);
			float min = V.col(1).minCoeff();
			float max = V.col(1).maxCoeff();

			targetScale /= std::abs(max - min);
		}

		V.array() *= targetScale;
	}

    void Harmonic(float* VPtr, int VSize, int* FPtr, int FSize) {
        auto V = Eigen::Map<V_t>(VPtr, VSize, 3);
        const auto F = Eigen::Map<F_t>(FPtr, FSize, 3);
        SparseV_t Q(VSize, 3);

        igl::harmonic(V, F, 2, Q);
        //V += Q;
    }
}