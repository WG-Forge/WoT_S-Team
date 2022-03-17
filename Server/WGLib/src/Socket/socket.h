#pragma once
#include "../defs.h"
#include "winsock2.h" // SOCKET, SOCKADDR_IN

namespace web
{
	namespace io_base
	{
		class socket
		{
		public:
			socket();
			bool init(byte s_b1, byte s_b2, byte s_b3, byte s_b4, ushort port);
			bool init(uint addres, ushort port);
			bool init(const char* addres, ushort port);
			bool init(const sockaddr_in& addr);
			bool bind();
			bool bind_before_connect();
			bool listen();
			bool listen(int members);
			const SOCKET& get_socket() const;
			const sockaddr_in& get_socket_address() const;
		private:
			SOCKET _socket;
			sockaddr_in _socket_address;
			bool _inited;
		};
	}
}