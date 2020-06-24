#pragma once
#include "Interface.h"

/**
 * Recalculates the boundary <code>state->Native->Boundary</code> if the relevant selections have changed
 * @param boundaryMask The current mask of selections part of the boundary
 * @return True if boundary has changed
 */
bool UpdateBoundary(State* state, unsigned int boundaryMask);

/**
 * Recalculates the boundary conditions <code>state->Native->BoundaryConditions</code> for Harmonic and Arap
 * @return True if the boundary conditions have changed
 */
bool UpdateBoundaryConditions(State* state);
