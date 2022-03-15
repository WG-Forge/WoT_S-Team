#pragma once

namespace ai
{
	struct player_native
	{
		int idx = 0;
		int is_observer = 0;
	};

	struct win_points_native
	{
		int id = 0;
		int capture = 0;
		int kill = 0;
	};

	struct attack_matrix_native
	{
		int id = 0;
		int attack[3]{};
	};
}