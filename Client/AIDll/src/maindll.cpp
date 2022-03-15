#include "AI/ai.h"
#include <exception>

extern "C"
{
#pragma region AI
	/////////////////// AI /////////////////////////////

	__declspec(dllexport) ai::ai* create_ai()
	{
		ai::ai* ai = nullptr;
		try
		{
			ai = new ai::ai();
			return ai;
		}
		catch (const std::exception&)
		{
			return nullptr;
		}
	}

	__declspec(dllexport) result get_action(ai::ai* ai,
		int curr_player,
		ai::player_native* players, int players_size,
		ai::vehicles_native* vehicle, int vehicle_size,
		ai::win_points_native* win_points, int win_points_size,
		ai::attack_matrix_native* attack_matrix, int attack_matrix_size,
		ai::point* base, int base_size,
		ai::point* obstacle, int obstacle_size,
		ai::point* light_repair, int light_repair_size,
		ai::point* hard_repair, int hard_repair_size,
		ai::point* catapult, int catapult_size,
		ai::point* catapult_usage, int catapult_usage_size,
		ai::action_ret* out_actions)
	{
		return ai->get_action(curr_player,
			players, players_size,
			vehicle, vehicle_size,
			win_points, win_points_size,
			attack_matrix, attack_matrix_size,
			base, base_size,
			obstacle, obstacle_size,
			light_repair, light_repair_size,
			hard_repair, hard_repair_size,
			catapult, catapult_size,
			catapult_usage, catapult_usage_size,
			out_actions);
	}

	__declspec(dllexport) result destroy_ai(ai::ai* ai)
	{
		if (!ai) return result::IVALID_PARAM;
		try
		{
			delete ai;
			return result::OKEY;
		}
		catch (const std::exception&)
		{
			return result::IVALID_PARAM;
		}
	}
	/////////////////// END AI /////////////////////////////
#pragma endregion
}