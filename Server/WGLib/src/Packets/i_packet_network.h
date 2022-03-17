#pragma once
#include "../defs.h"
//#pragma warning(disable : 4200) // byte packet[0];

#define WEB_BASE_PACKET_NETWORK_MAX_SIZE 40960
#define WEB_BASE_PACKET_MAX_SIZE (WEB_BASE_PACKET_NETWORK_MAX_SIZE - sizeof(web::packet::packet_header))
#define WEB_BASE_PACKET_MIN_SIZE (sizeof(web::packet::packet_header))

namespace web
{
	namespace packet
	{
		struct packet_header
		{
			int32 type;
			int32 size;
		};

		struct packet_network
		{
			packet_header header;
			byte body[WEB_BASE_PACKET_MAX_SIZE];
		};

		struct i_packet_network
		{
			const packet_header& get_header()
			{
				return _header;
			}
		protected:
			packet_header _header{0,0};
		};
	}
}