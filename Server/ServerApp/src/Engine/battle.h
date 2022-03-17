#pragma once
#include "../Model/model.h"
#include "../../../WGserver/src/include.h"
#include "../Json/json_extensions.h"
#include <chrono>
#include <set>
#define TIME_STEP 10000

namespace engine
{
	struct vehicle_param
	{
		int hp_max = 0;
		int speed = 0;
		int damage = 0;
		int gold = 0;
		int shoot_min = 0;
		int shoot_max = 0;
	};

	struct post_action
	{
		web::io_base::i_connection* conn;
		models::result result;
		std::string json_str;
	};

	class battle
	{
	public:
		battle(std::string name, int num_players, int num_turns);

		bool add_player(const models::player& player, web::io_base::i_connection* conn);
		bool update_player(const models::player& player, web::io_base::i_connection* conn);
		const std::string& get_name();
		const models::map& get_map();
		const models::game_state& get_game_state();
		const models::action_rsp& get_actions();
		models::player get_player_by_name(const std::string name);
		models::result set_turn(web::io_base::i_connection* conn);
		models::result set_move(const models::move& move, web::io_base::i_connection* conn);
		models::result set_shoot(const models::shoot& shoot, web::io_base::i_connection* conn);

		friend class engine;
	private:
		int _vehicle_id = 1;
		vehicle_param _vehicle_params[5]
		{
			{1,1,1,1,3,3},
			{1,3,1,1,2,2},
			{3,1,1,3,1,2},
			{2,2,1,2,2,2},
			{2,1,1,2,1,3},
		};
		std::chrono::milliseconds _time;
		std::string _name{};
		models::game_state _game_state;
		models::map _map;
		models::action_rsp _actions;
		std::vector<web::io_base::i_connection*> _player_conn;
		std::vector<web::io_base::i_connection*> _observer_conn;

		std::vector<web::io_base::i_connection*> _waiters;
		std::set<int> _actioned;

		void _init_map();
		void _init_game_state();
		models::vehicle _create_vehicle(models::vehicle_type type, int player_index, const models::point3& pos);
		void _add_vehicles(int player_index);
		models::player _get_next_player();
		int _get_player_index(web::io_base::i_connection* conn);
		bool _vehicle_is_eat(int veh_id);
		std::vector<models::point3> _get_point_around(const models::point3& point, int n, int N);
		std::vector<models::point3> _get_points_move(const models::point3& point, int speed);
		bool _get_vector(const models::point3& src, const models::point3& dst, int offset, models::point3& out);
		std::vector<int> _get_path_shoot_pt(int player_id, const models::point3& point, const models::point3& target, int shoot_max);
		std::vector<int> _get_path_shoot(int player_id, const models::point3& target);
		bool _can_move(const models::vehicle& vehicle, const models::point3& point);
		void _shoot(int player_index, int vehicle_id);
		std::vector<int> _can_shoot(const models::vehicle& vehicle, const models::point3& point);
		void _add_capture_points();
		void _add_stubs();
		bool _check_actioned(int veh_id);
		std::vector<post_action> _next();
	};
}
