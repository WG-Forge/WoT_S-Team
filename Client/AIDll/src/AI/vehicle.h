#pragma once
#include "../defs.h"

namespace ai
{
	enum class vehicle_type : int
	{
		MT,
		LT,
		HT,
		ASPG,
		SPG
	};

	struct vehicles_native
	{
		int vehicle_id = 0;
		int player_id = 0;
		vehicle_type vehicle_type = vehicle_type::MT;
		int health = 0;
		point spawn_position{};
		point position{};
		int capture_points = 0;
		int shoot_range_bonus;
	};
}