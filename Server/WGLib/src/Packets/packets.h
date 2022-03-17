#pragma once
#include "../defs.h"
#include "i_packet_network.h"

#include <memory>

namespace web
{
	namespace packet
	{
		template<typename T>
		T* packet_cast(packet_network* p)
		{
			return reinterpret_cast<T*>(p);
		}

		class packet_string : public web::packet::i_packet_network
		{
		public:
			packet_string(const char* s)
			{
				_header.type = 0;
				if (s == nullptr) return;
				if (!memcpy_s(str, sizeof(str), s, strlen(s) + sizeof(char)))
					_header.size += strlen(s) + sizeof(char);
			}
			char str[WEB_BASE_PACKET_MAX_SIZE];
		};
	}
}
