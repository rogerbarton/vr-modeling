#include "Interface.h"
#include "InterfaceTypes.h"
#include "NumericTypes.h"

extern "C" {
	/**
	 * Example using MeshDataNative
	 * @param dirtyState <code>DirtyFlag</code> as an unsigned integer so bitmask operations can be done easily
	 */
	void CustomUpdateSample(const MeshDataNative data, unsigned int& dirtyState)
	{
		auto V = Eigen::Map<V_t>(data.V, data.VSize, 3);
		// Modify V ...
		dirtyState |= DirtyFlag::VDirty;
	}
}