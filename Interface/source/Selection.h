#pragma once

struct Selection
{
	/**
	 * @return Returns the bit-shifted mask, if selectionId is -1 mask is zero
	 */
	static unsigned int GetMask(int selectionId);
};