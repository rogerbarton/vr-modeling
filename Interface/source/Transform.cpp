#include "Interface.h"

// --- Transformations
void TranslateAllVertices(State* state, Vector3 value)
{
	state->V->rowwise() += value.AsEigenRow();
	state->DirtyState |= DirtyFlag::VDirty;
}

void TranslateSelection(State* state, Vector3 value, unsigned int maskId)
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

void TransformSelection(State* state, Vector3 translation, float scale, Quaternion rotation, unsigned int maskId)
{
	auto& V = *state->V;
	const auto& S = *state->S;

	using namespace Eigen;
	Transform<float, 3, Affine> transform =
			Translation3f(translation.AsEigen()) * rotation.AsEigen() * Scaling(scale);

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

void ResetV(State* state)
{
	*state->V = *state->Native->V0;
	state->DirtyState |= DirtyFlag::VDirty;
}
