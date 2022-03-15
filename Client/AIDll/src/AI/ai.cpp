#include "ai.h"
#include <cmath>
#include <algorithm>
#include <fstream>
#include <vector>
#include <limits>

namespace ai
{
	result ai::get_action(int curr_player,
		player_native* players, int players_size,
		vehicles_native* vehicles, int vehicles_size,
		win_points_native* win_points, int win_points_size,
		attack_matrix_native* attack_matrix, int attack_matrix_size,
		point* base, int base_size,
		point* obstacle, int obstacle_size,
		point* light_repair, int light_repair_size,
		point* hard_repair, int hard_repair_size,
		point* catapult, int catapult_size,
		point* catapult_usage, int catapult_usage_size,
		action_ret* out_actions)
	{

		//Initialization of vectors of input
		std::vector<vehicles_native> vehicles_array(vehicles, vehicles + vehicles_size);
		std::vector<point> obstacles_array(obstacle, obstacle + obstacle_size);
		std::vector<attack_matrix_native> attack_matrix_array(attack_matrix, attack_matrix + attack_matrix_size);
		std::vector<vehicles_native> tanks(vehicles, vehicles + ALLY_TANKS_NUMBER);

		get_tanks_in_order(curr_player, vehicles_array, tanks);

		if (key_positions.empty()) {
			for (int i = 0; i < ALLY_TANKS_NUMBER; i++)
				key_positions.push_back(tanks[i].spawn_position);

			set_vehicles_positions(tanks, obstacles_array);
		}

		for (int i = 0; i < ALLY_TANKS_NUMBER; i++) {
			auto act = check_for_shooting(curr_player, vehicles_array, tanks[i], attack_matrix_array);
			out_actions->actions[i] = act;
			if (act.action_type == action_type::shoot) {
				continue;
			}
			act = move_tank(vehicles_array, tanks, tanks[i], obstacles_array);
			out_actions->actions[i] = act;
		}

		return result::OKEY;
	}

	point ai::get_hex_sum(const point& hex, const point& vec) {
		return point{ hex.x + vec.x, hex.y + vec.y, hex.z + vec.z };
	}

	point ai::get_hex_neighbor(const point& hex, int direction) {
		return get_hex_sum(hex, hex_direction_vectors[direction]);
	}

	void ai::get_tanks_in_order(int curr_player, const std::vector<vehicles_native>& vehicles,
		std::vector<vehicles_native>& result)
	{

		for (size_t i = 0; i < vehicles.size(); i++) {
			if (vehicles[i].player_id != curr_player) {
				continue;
			}
			if (vehicles[i].vehicle_type == vehicle_type::SPG) {
				result[SPG_POSITION_INDEX] = vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::LT) {
				result[LT_POSITION_INDEX] = vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::HT) {
				result[HT_POSITION_INDEX] = vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::MT) {
				result[MT_POSITION_INDEX] = vehicles[i];
			}
			else if (vehicles[i].vehicle_type == vehicle_type::ASPG) {
				result[ASPG_POSITION_INDEX] = vehicles[i];
			}
		}
	}

	action ai::check_for_shooting(int curr_player, std::vector<vehicles_native>& vehicles,
		const vehicles_native& vehicle, const std::vector<attack_matrix_native>& attack_matrix)
	{

		action to_return;
		to_return.action_type = action_type::nun;

		if (vehicle.vehicle_type != vehicle_type::ASPG) {

			std::vector<vehicles_native*> targets;
			for (size_t i = 0; i < vehicles.size(); i++) {
				if (check_the_shooting_zone(vehicle, vehicles[i]) &&
					check_neutrality(curr_player, vehicles[i].player_id, attack_matrix)) {
					targets.push_back(&vehicles[i]);
				}
			}

			std::sort(targets.begin(), targets.end(), [](vehicles_native* l, vehicles_native* r)
				{
					return l->health < r->health ||
						l->health == r->health && l->capture_points > r->capture_points;
				});

			if (!targets.empty()) {
				to_return.action_type = action_type::shoot;
				to_return.vec_id = vehicle.vehicle_id;
				to_return.point = targets[0]->position;
				targets[0]->health--;
			}

		}
		else {

			struct direction_parameters
			{
				point target;
				int sum_capture_points;
				int sum_destruct_points;
				int sum_health;
				std::vector<vehicles_native*> vehicles;
				direction_parameters(point target) : target(target), sum_capture_points(0), sum_destruct_points(0),
					sum_health(0), vehicles() {}
			};

			std::vector<direction_parameters> params;
			point current_position = vehicle.position;
			params.push_back(direction_parameters(
				point{ current_position.x, current_position.y + 1, current_position.z - 1 })); // for x forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x, current_position.y - 1, current_position.z + 1 })); // for x forwarding down
			params.push_back(direction_parameters(
				point{ current_position.x + 1, current_position.y, current_position.z - 1 })); // for y forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x - 1, current_position.y, current_position.z + 1 })); // for y forwarding down
			params.push_back(direction_parameters(
				point{ current_position.x - 1, current_position.y + 1, current_position.z })); // for z forwarding up
			params.push_back(direction_parameters(
				point{ current_position.x + 1, current_position.y - 1, current_position.z })); // for z forwarding down

			for (size_t i = 0; i < vehicles.size(); i++) {
				if (curr_player == vehicles[i].player_id ||
					!check_neutrality(curr_player, vehicles[i].player_id, attack_matrix) ||
					vehicles[i].health == 0) {
					continue;
				}
				int dist = distance(vehicle.position, vehicles[i].position);
				if (dist <= MAX_SHOOT_RANGE) {
					if (vehicle.position.x == vehicles[i].position.x) {
						if (vehicle.position.y < vehicles[i].position.y) {
							params[0].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[0].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[0].sum_health++;
							params[0].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[1].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[1].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[1].sum_health++;
							params[1].vehicles.push_back(&vehicles[i]);
						}
					}
					if (vehicle.position.y == vehicles[i].position.y) {
						if (vehicle.position.x < vehicles[i].position.x) {
							params[2].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[2].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[2].sum_health++;
							params[2].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[3].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[3].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[3].sum_health++;
							params[3].vehicles.push_back(&vehicles[i]);
						}
					}
					if (vehicle.position.z == vehicles[i].position.z) {
						if (vehicle.position.x > vehicles[i].position.x) {
							params[4].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[4].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[4].sum_health++;
							params[4].vehicles.push_back(&vehicles[i]);
						}
						else {
							params[5].sum_capture_points += vehicles[i].capture_points;
							if (vehicles[i].health == 1) {
								params[5].sum_destruct_points += get_destruct_points(vehicles[i].vehicle_type);
							}
							params[5].sum_health++;
							params[5].vehicles.push_back(&vehicles[i]);
						}
					}
				}
			}

			std::sort(params.begin(), params.end(), [](direction_parameters l, direction_parameters r) {
				return l.sum_capture_points > r.sum_capture_points ||
					l.sum_capture_points == r.sum_capture_points && l.sum_destruct_points > r.sum_destruct_points ||
					l.sum_capture_points == r.sum_capture_points && l.sum_destruct_points == r.sum_destruct_points &&
					l.sum_health > r.sum_health;
				});

			if (params[0].sum_health != 0) {
				to_return.action_type = action_type::shoot;
				to_return.vec_id = vehicle.vehicle_id;
				to_return.point = params[0].target;
				for (auto& veh : params[0].vehicles) {
					veh->health--;
				}
			}

		}

		return to_return;
	}

	void ai::get_reachable_hexes(std::vector<std::vector<point>>& points,
		const std::vector<point>& obstacles, const std::vector<vehicles_native>& vehicles,
		const point& start, int speed)
	{

		std::vector<point> visited;
		visited.push_back(start);
		points.push_back(std::vector<point>());
		points[0].push_back(start);

		std::vector<point> vehicles_positions;
		for (size_t i = 0; i < vehicles.size(); i++)
			vehicles_positions.push_back(vehicles[i].position);

		for (int i = 1; i < speed + 1; i++) {
			points.push_back(std::vector<point>());
			for (const point& pts : points[i - 1]) {
				for (int dir = 0; dir < 6; dir++) {
					point neighbour = get_hex_neighbor(pts, dir);
					if (std::find(visited.begin(), visited.end(), neighbour) == visited.end() &&
						std::find(obstacles.begin(), obstacles.end(), neighbour) == obstacles.end() &&
						std::find(vehicles_positions.begin(), vehicles_positions.end(), neighbour) == 
						vehicles_positions.end()) {

						visited.push_back(neighbour);
						points[i].push_back(neighbour);
					}
				}
			}
		}
	}

	void ai::set_vehicles_positions(const std::vector<vehicles_native>& vehicles,
		const std::vector<point>& obstacles)
	{

		int min_dist = std::numeric_limits<int>::max();
		int dist;
		std::vector<point> base;

		//find MT and HT positions (in base)
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			base.push_back(hex_direction_vectors[dir]);
			dist = distance(base[dir], vehicles[2].spawn_position);
			if (dist < min_dist) {
				min_dist = dist;
				key_positions[2] = base[dir];
			}
			else if (dist == min_dist) {
				key_positions[3] = base[dir];
			}
		}
		base.push_back(point{});

		//find LT position (near base and enemy) and SPG position (near base and allies)
		point neighbour;
		min_dist = 0;
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			neighbour = get_hex_neighbor(key_positions[2], dir);

			if (std::find(base.begin(), base.end(), neighbour) != base.end() ||
				std::find(obstacles.begin(), obstacles.end(), neighbour) != obstacles.end())
				continue;

			dist = distance(neighbour, vehicles[3].spawn_position);
			if (dist > min_dist) {
				min_dist = dist;
				key_positions[0] = key_positions[1];
				key_positions[1] = neighbour;
			}
			else
				key_positions[0] = neighbour;
		}

		//find ASPG position (far from base to shoot one side of base)
		for (int dir = 0; dir < DIRECTIONS_NUMBER; dir++) {
			neighbour = get_hex_neighbor(key_positions[0], dir);

			if (std::find(base.begin(), base.end(), neighbour) != base.end() ||
				std::find(obstacles.begin(), obstacles.end(), neighbour) != obstacles.end())
				continue;

			if (key_positions[3].x == neighbour.x ||
				key_positions[3].y == neighbour.y ||
				key_positions[3].z == neighbour.z) {

				key_positions[4] = neighbour;
				break;
			}
		}
	}

	action ai::move_tank(const std::vector<vehicles_native>& vehicles,
		const std::vector<vehicles_native>& ally_vehicles, const vehicles_native& vehicle,
		const std::vector<point>& obstacles)
	{

		action return_action;
		return_action.action_type = action_type::move;
		return_action.vec_id = vehicle.vehicle_id;
		return_action.point = vehicle.position;

		point goal_position_LT = key_positions[LT_POSITION_INDEX];
		int vehicle_goal_position = SPG_POSITION_INDEX;

		switch (vehicle.vehicle_type) {
		case vehicle_type::SPG: {
			if (distance(ally_vehicles[HT_POSITION_INDEX].position, key_positions[HT_POSITION_INDEX]) >=
				distance(vehicle.position, key_positions[HT_POSITION_INDEX])) {
				return_action.action_type = action_type::nun;
				return return_action;
			}
		} break;
		case vehicle_type::LT: {
			vehicle_goal_position = LT_POSITION_INDEX;
			if (distance(ally_vehicles[HT_POSITION_INDEX].position, key_positions[HT_POSITION_INDEX]) > 1)
				key_positions[LT_POSITION_INDEX] = key_positions[HT_POSITION_INDEX];
		} break;
		case vehicle_type::HT: {
			vehicle_goal_position = HT_POSITION_INDEX;
		} break;
		case vehicle_type::MT: {
			vehicle_goal_position = MT_POSITION_INDEX;
		} break;
		case vehicle_type::ASPG: {
			vehicle_goal_position = ASPG_POSITION_INDEX;
		} break;
		}

		if (distance(vehicle.position, key_positions[vehicle_goal_position]) == 0) {
			if (key_positions[LT_POSITION_INDEX] == key_positions[HT_POSITION_INDEX])
				key_positions[LT_POSITION_INDEX] = goal_position_LT;
			return_action.action_type = action_type::nun;
			return return_action;
		}

		int speed = get_speed(vehicle.vehicle_type);
		std::vector<std::vector<point>> reachable_points;
		get_reachable_hexes(reachable_points, obstacles, vehicles, vehicle.position, speed);

		if (vehicle.vehicle_type == vehicle_type::ASPG) {
			int com = 0;
		}
		while (speed > 0) {
			if (reachable_points[speed].empty())
				speed--;
			else
				break;
		}

		if (speed == 0) {
			if (key_positions[LT_POSITION_INDEX] == key_positions[HT_POSITION_INDEX])
				key_positions[LT_POSITION_INDEX] = goal_position_LT;
			return_action.action_type = action_type::nun;
			return return_action;
		}

		int dist;
		int min_dist = std::numeric_limits<int>::max();
		for (int i = 1; i < speed + 1; i++) {
			for (const point& pts : reachable_points[i]) {
				dist = distance(pts, key_positions[vehicle_goal_position]);
				if (dist <= min_dist) {
					min_dist = dist;
					return_action.point = pts;
				}
			}
		}

		if (key_positions[LT_POSITION_INDEX] == key_positions[HT_POSITION_INDEX])
			key_positions[LT_POSITION_INDEX] = goal_position_LT;

		return return_action;
	}

	int ai::get_destruct_points(const vehicle_type& type)
	{

		if (type == vehicle_type::SPG || type == vehicle_type::LT) {
			return 1;
		}
		if (type == vehicle_type::MT || type == vehicle_type::ASPG) {
			return 2;
		}
		if (type == vehicle_type::HT) {
			return 3;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	int ai::get_speed(const vehicle_type& type)
	{

		if (type == vehicle_type::SPG || type == vehicle_type::HT || type == vehicle_type::ASPG) {
			return 1;
		}
		if (type == vehicle_type::MT) {
			return 2;
		}
		if (type == vehicle_type::LT) {
			return 3;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	bool ai::check_the_shooting_zone(const vehicles_native& shooter, const vehicles_native& goal)
	{

		if (goal.health == 0 || shooter.player_id == goal.player_id) {
			return false;
		}

		if (shooter.vehicle_type == vehicle_type::SPG) {
			return distance(shooter.position, goal.position) == 3;
		}
		if (shooter.vehicle_type == vehicle_type::LT || shooter.vehicle_type == vehicle_type::MT) {
			return distance(shooter.position, goal.position) == 2;
		}
		if (shooter.vehicle_type == vehicle_type::HT) {
			return distance(shooter.position, goal.position) > 0 && distance(shooter.position, goal.position) <= 2;
		}
		throw std::invalid_argument("vehicle_type error");
	}

	int ai::distance(const point& a, const point& b)
	{
		return (abs(a.x - b.x) + abs(a.y - b.y) + abs(a.z - b.z)) / 2;
	}

	bool ai::check_neutrality(int curr_player, int goal, const std::vector<attack_matrix_native>& attack_matrix)
	{

		for (const attack_matrix_native& shoot : attack_matrix) {
			if (shoot.id != goal) {
				continue;
			}
			for (int j = 0; j < 3; j++) {
				if (shoot.attack[j] == curr_player) {
					return true;
				}
			}
		}
		bool was_attacked_by_third = false;
		for (const attack_matrix_native& shoot : attack_matrix) {
			for (int j = 0; j < 3; j++) {
				if (shoot.attack[j] == goal && shoot.id != curr_player) {
					was_attacked_by_third = true;
				}
			}
		}
		return !was_attacked_by_third;
	}
}