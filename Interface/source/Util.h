#pragma once
#include "InterfaceTypes.h"

namespace Interface
{
	/**
	 * Set the center/origin of the mesh to be the mean vertex
	 * @tparam V_T Type of the vertex matrix to support both Col and RowMajor
	 */
	template<typename V_T>
	void CenterToMean(float* VPtr, int VSize)
	{
		assert(VPtr != nullptr);

		auto V = Eigen::Map<V_T>(VPtr, VSize, 3);
		Eigen::RowVector3f mean = V.colwise().mean();
		V.rowwise() -= mean;
	}

	/**
	 * Applies the scale of a mesh to the vertices, i.e. cwise multiply.
	 * If <code>targetScale</code> is set to zero, the model scale is normalized so it has unit height.
	 * @param centerToMean If true sets the center to the mean vertex.
	 * Note: y-axis center will be the center of the bounding box for easier positioning
	 * @param normalize If set to true the absolute y-height of the model will be <code>targetScale</code> otherwise only
	 * the targetScale factor is applied.
	 * @tparam V_T Type of the vertex matrix to support both Col and RowMajor
	 */
	template<typename V_T>
	void ApplyScale(float* VPtr, int VSize, bool centerToMean = true, bool normalize = true, float targetScale = 1.f)
	{
		assert(VPtr != nullptr);
		auto V = Eigen::Map<V_T>(VPtr, VSize, 3);

		if (centerToMean || normalize)
		{
			float miny = V.col(1).minCoeff();
			float maxy = V.col(1).maxCoeff();

			if (centerToMean)
			{
				// Make mean center, y-axis make center the center of bounding box for easier positioning
				Eigen::RowVector3f mean = V.colwise().mean();
				Eigen::RowVector3f offset;
				offset << mean(0), (miny + maxy) / 2.f, mean(2);
				V.rowwise() -= offset;
			}

			if (normalize)
				targetScale /= std::abs(maxy - miny);
		}

		V.array() *= targetScale;
	}


	using Color_t = Eigen::RowVector4f;

	struct Color
	{
		static Color_t White;
		static Color_t Black;
		static Color_t Red;
		static Color_t Green;
		static Color_t Blue;
		static Color_t Orange;
		static Color_t Purple;
		static Color_t GreenLight;
		static Color_t BlueLight;
		static Color_t Yellow;

		static const Color_t& GetColorById(int selectionId);
	};
}