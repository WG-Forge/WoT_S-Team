#pragma once
#include "../../../WGLib/src/Packets/packets.h"
#include "../Model/model.h"

namespace packets
{
	class packet_error : public web::packet::i_packet_network
	{
	public:
		packet_error(models::result res);
	};

	class packet_json : public web::packet::i_packet_network
	{
	public:
		packet_json(models::result res, std::string json);
		byte json_str[WEB_BASE_PACKET_MAX_SIZE];
	};
}
