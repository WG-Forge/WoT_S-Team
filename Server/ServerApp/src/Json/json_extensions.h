#pragma once
#include "../Json/json.hpp"
#include "../Model/model.h"

namespace models
{
	void to_json(nlohmann::json& j, const player& p);
	void from_json(const nlohmann::json& j, player& p);

	void to_json(nlohmann::json& j, const login& p);
	void from_json(const nlohmann::json& j, login& p);

	void to_json(nlohmann::json& j, const point3& p);
	void from_json(const nlohmann::json& j, point3& p);

	void to_json(nlohmann::json& j, const content& p);
	void from_json(const nlohmann::json& j, content& p);

	void to_json(nlohmann::json& j, const spawn_points& p);
	void from_json(const nlohmann::json& j, spawn_points& p);

	void to_json(nlohmann::json& j, const map& p);
	void from_json(const nlohmann::json& j, map& p);

	NLOHMANN_JSON_SERIALIZE_ENUM(vehicle_type,
		{
			{medium_tank, "medium_tank"},
			{light_tank, "light_tank"},
			{heavy_tank, "heavy_tank"},
			{at_spg, "at_spg"},
			{spg, "spg"}
		});

	void to_json(nlohmann::json& j, const vehicle& p);
	void from_json(const nlohmann::json& j, vehicle& p);

	void to_json(nlohmann::json& j, const win_points& p);
	void from_json(const nlohmann::json& j, win_points& p);

	void to_json(nlohmann::json& j, const game_state& p);
	void from_json(const nlohmann::json& j, game_state& p);

	void to_json(nlohmann::json& j, const action_rsp& p);
	void from_json(const nlohmann::json& j, action_rsp& p);

	void to_json(nlohmann::json& j, const move& p);
	void from_json(const nlohmann::json& j, move& p);

	void to_json(nlohmann::json& j, const shoot& p);
	void from_json(const nlohmann::json& j, shoot& p);
}