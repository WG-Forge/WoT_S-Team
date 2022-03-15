#pragma once
#include "../defs.h"

namespace ai
{
	enum class action_type : int
	{
		nun,
		move,
		shoot
	};

	struct action
	{
		action_type action_type = action_type::nun;
		int vec_id = 0;
		point point{};
	};

	struct action_ret
	{
		action actions[5]{};
	};
}
