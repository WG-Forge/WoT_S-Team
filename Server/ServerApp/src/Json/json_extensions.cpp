#include "json_extensions.h"

namespace models
{
	void to_json(nlohmann::json& j, const player& p)
	{
		j = nlohmann::json{
			{"idx", p.idx},
			{"name", p.name},
			{"is_observer", p.is_observer}
		};
	}

	void from_json(const nlohmann::json& j, player& p)
	{
		j.at("idx").get_to(p.idx);
		j.at("name").get_to(p.name);
		j.at("is_observer").get_to(p.is_observer);
	}


	void to_json(nlohmann::json& j, const login& p)
	{
		j = nlohmann::json{
			{"name", p.name},
			{"password", p.password},
			{"game", p.game},
			{"num_turns", p.num_turns},
			{"num_players", p.num_players},
			{"is_observer", p.is_observer},
		};
	}

	void from_json(const nlohmann::json& j, login& p)
	{
		j.at("name").get_to(p.name);

		auto j_password = j.at("password");
		auto j_game = j.at("game");
		auto j_num_turns = j.at("num_turns");
		auto j_num_players = j.at("num_players");
		auto j_is_observer = j.at("is_observer");

		if (j_password.is_null()) p.password = ""; else j_password.get_to(p.password);
		if (j_game.is_null()) p.game = ""; else j_game.get_to(p.game);
		if (j_num_turns.is_null()) p.num_turns = 45; else j_num_turns.get_to(p.num_turns);
		if (j_num_players.is_null()) p.num_players = 1; else j_num_players.get_to(p.num_players);
		if (j_is_observer.is_null()) p.is_observer = false; else j_is_observer.get_to(p.is_observer);
	}


	void to_json(nlohmann::json& j, const point3& p)
	{
		j = nlohmann::json{
			{"x", p.x},
			{"y", p.y},
			{"z", p.z}
		};
	}
	void from_json(const nlohmann::json& j, point3& p)
	{
		j.at("x").get_to(p.x);
		j.at("y").get_to(p.y);
		j.at("z").get_to(p.z);
	}

	void to_json(nlohmann::json& j, const content& p)
	{
		j = nlohmann::json{
			{"base", p.base},
			{"obstacle", p.obstacle},
			{"light_repair", p.light_repair},
			{"hard_repair", p.hard_repair},
			{"catapult", p.catapult}
		};
	}

	void from_json(const nlohmann::json& j, content& p)
	{
		j.at("base").get_to<std::vector<point3>>(p.base);
		j.at("obstacle").get_to<std::vector<point3>>(p.obstacle);
		j.at("light_repair").get_to<std::vector<point3>>(p.light_repair);
		j.at("hard_repair").get_to<std::vector<point3>>(p.hard_repair);
		j.at("catapult").get_to<std::vector<point3>>(p.catapult);
	}


	void to_json(nlohmann::json& j, const spawn_points& p)
	{
		j = nlohmann::json{
			{"medium_tank", p.medium_tank},
			{"light_tank", p.light_tank},
			{"heavy_tank", p.heavy_tank},
			{"at_spg", p.at_spg},
			{"spg", p.spg}
		};
	}

	void from_json(const nlohmann::json& j, spawn_points& p)
	{
		j.at("medium_tank").get_to<std::vector<point3>>(p.medium_tank);
		j.at("light_tank").get_to<std::vector<point3>>(p.light_tank);
		j.at("heavy_tank").get_to<std::vector<point3>>(p.heavy_tank);
		j.at("at_spg").get_to<std::vector<point3>>(p.at_spg);
		j.at("spg").get_to<std::vector<point3>>(p.spg);
	}

	void to_json(nlohmann::json& j, const map& p)
	{
		j = nlohmann::json{
			{"size", p.size},
			{"name", p.name},
			{"spawn_points", p.spawn_points},
			{"content", p.content}
		};
	}

	void from_json(const nlohmann::json& j, map& p)
	{
		j.at("size").get_to(p.size);
		j.at("name").get_to(p.name);
		j.at("spawn_points").get_to<std::vector<spawn_points>>(p.spawn_points);
		j.at("content").get_to(p.content);
	}

	void to_json(nlohmann::json& j, const vehicle& p)
	{
		j = nlohmann::json{
			{"player_id", p.player_id},
			{"vehicle_type", p.vehicle_type},
			{"health", p.health},
			{"spawn_position", p.spawn_position},
			{"position", p.position},
			{"capture_points", p.capture_points},
			{"shoot_range_bonus", p.shoot_range_bonus}
		};
	}

	void from_json(const nlohmann::json& j, vehicle& p)
	{
		j.at("player_id").get_to(p.player_id);
		j.at("vehicle_type").get_to(p.vehicle_type);
		j.at("health").get_to(p.health);
		j.at("spawn_position").get_to(p.spawn_position);
		j.at("position").get_to(p.position);
		j.at("capture_points").get_to(p.capture_points);
		j.at("shoot_range_bonus").get_to(p.shoot_range_bonus);
	}

	void to_json(nlohmann::json& j, const win_points& p)
	{
		j = nlohmann::json{
			{"capture", p.capture},
			{"kill", p.kill}
		};
	}

	void from_json(const nlohmann::json& j, win_points& p)
	{
		j.at("capture").get_to(p.capture);
		j.at("kill").get_to(p.kill);
	}

	void to_json(nlohmann::json& j, const game_state& p)
	{
		nlohmann::json arr_win_points;
		for (auto it = p.win_points.begin(); it != p.win_points.end(); ++it)
			arr_win_points[std::to_string(it->first)] = it->second;

		nlohmann::json arr_vehicles;
		for (auto it = p.vehicles.begin(); it != p.vehicles.end(); ++it)
			arr_vehicles[std::to_string(it->first)] = it->second;
	
		nlohmann::json arr_attack;
		for (auto it = p.attack_matrix.begin(); it != p.attack_matrix.end(); ++it)
			arr_attack[std::to_string(it->first)] = it->second;

		j = nlohmann::json{
			{"num_players", p.num_players},
			{"num_turns", p.num_turns},
			{"current_turn", p.current_turn},
			{"players", p.players},
			{"observers", p.observers},
			{"current_player_idx", p.current_player_idx},
			{"finished", p.finished},
			{"vehicles", arr_vehicles},
			{"attack_matrix", arr_attack},
			{"winner", p.winner},
			{"win_points", arr_win_points},
			{"catapult_usage", p.catapult_usage}
		};

		if (p.current_player_idx == 0)
			j["current_player_idx"] = nullptr;
		if (p.winner == 0)
			j["winner"] = nullptr;
	}

	void from_json(const nlohmann::json& j, game_state& p)
	{
		j.at("num_players").get_to(p.num_players);
		j.at("num_turns").get_to(p.num_turns);
		j.at("current_turn").get_to(p.current_turn);
		j.at("players").get_to<std::vector<player>>(p.players);
		j.at("observers").get_to<std::vector<player>>(p.observers);
		j.at("current_player_idx").get_to(p.current_player_idx);
		j.at("finished").get_to(p.finished);
		j.at("vehicles").get_to<std::map<int, vehicle>>(p.vehicles);

		j.at("attack_matrix").get_to<std::map<int, std::vector<int>>>(p.attack_matrix);

		j.at("winner").get_to(p.winner);
		j.at("win_points").get_to<std::map<int, win_points>>(p.win_points);
		j.at("catapult_usage").get_to(p.catapult_usage);
	}

	void to_json(nlohmann::json& j, const action_rsp& p)
	{
		j = nlohmann::json{
			{"actions", p.actions}
		};
	}

	void from_json(const nlohmann::json& j, action_rsp& p)
	{
		j.at("capture").get_to(p.actions);
	}

	void to_json(nlohmann::json& j, const move& p)
	{
		j = nlohmann::json{
			{"vehicle_id", p.vehicle_id},
			{"target", p.target}
		};
	}

	void from_json(const nlohmann::json& j, move& p)
	{
		j.at("vehicle_id").get_to(p.vehicle_id);
		j.at("target").get_to(p.target);
	}

	void to_json(nlohmann::json& j, const shoot& p)
	{
		j = nlohmann::json{
			{"vehicle_id", p.vehicle_id},
			{"target", p.target}
		};
	}

	void from_json(const nlohmann::json& j, shoot& p)
	{
		j.at("vehicle_id").get_to(p.vehicle_id);
		j.at("target").get_to(p.target);
	}
}