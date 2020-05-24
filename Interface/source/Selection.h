#pragma once

struct Selection{
	/**
	 * @param selectionId
	 * @return Returns the bit-shifted mask, if @refitem selectionId is -1 mask is zero
	 */
	static unsigned int GetMask(unsigned int selectionId);
};