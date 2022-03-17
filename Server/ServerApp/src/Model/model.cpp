#include "model.h"

namespace models
{
	std::vector<point3>& spawn_points::get_spawn_points_by_type(vehicle_type type)
	{
		if (type == vehicle_type::medium_tank) return medium_tank;
		if (type == vehicle_type::light_tank) return light_tank;
		if (type == vehicle_type::heavy_tank) return heavy_tank;
		if (type == vehicle_type::at_spg) return at_spg;
		return spg;
	}
}
