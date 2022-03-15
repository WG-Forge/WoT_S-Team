#pragma once
#include "../../WebDll/src/defs.h"

namespace ai
{
	struct point
	{
		int x = 0, y = 0, z = 0;
	};

	inline bool operator==(const point& right, const point& left) {
		if (left.x == right.x && left.y == right.y && left.z == right.z)
			return true;
		else
			return false;
	}
}