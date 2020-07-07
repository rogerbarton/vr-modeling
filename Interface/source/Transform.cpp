#include "Native.h"

// --- Transformations
void TranslateAllVertices(MeshState* state, Vector3 value)
{
	state->V->rowwise() += value.AsEigenRow();
	state->DirtyState |= DirtyFlag::VDirty;
}

void TranslateSelection(MeshState* state, Vector3 value, unsigned int maskId)
{
	auto& V = *state->V;
	const auto& S = *state->S;
	const Eigen::RowVector3f valueEigen = value.AsEigenRow();

	for (int i = 0; i < V.rows(); ++i)
	{
		if ((S(i) & maskId) > 0)
			V.row(i) += valueEigen;
	}

	state->DirtyState |= DirtyFlag::VDirty;
}

void TransformSelection(MeshState* state, Vector3 translation, float scale, Quaternion rotation, Vector3 pivot, unsigned int maskId)
{
	auto& V = *state->V;
	const auto& S = *state->S;

	using namespace Eigen;
	Transform<float, 3, Affine> transform =
			Translation3f(translation.AsEigen()) *
			Translation3f(pivot.AsEigen()) * Scaling(scale) * rotation.AsEigen() * Translation3f(-pivot.AsEigen());

	for (int i = 0; i < V.rows(); ++i)
	{
		if ((S(i) & maskId) > 0)
		{
			Vector3f v = V.row(i);
			V.row(i) = transform * v;
		}
	}

	state->DirtyState |= DirtyFlag::VDirty;
}

void ResetV(MeshState* state)
{
	*state->V = *state->Native->V0;
	state->DirtyState |= DirtyFlag::VDirty;
}
