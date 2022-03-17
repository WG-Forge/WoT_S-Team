#include "packets.h"
#include <string>
#pragma warning(disable : 4996)

namespace packets
{
	packet_error::packet_error(models::result res)
	{
		_header.type = static_cast<int32>(res);
	}

	packet_json::packet_json(models::result res, std::string json)
	{
		_header.type = static_cast<int32>(res);
		if (json.empty()) return;
		strncpy(reinterpret_cast<char*>(json_str), json.c_str(), sizeof(json_str));
		_header.size = strlen((char*)json_str) + 1;
	}
}
