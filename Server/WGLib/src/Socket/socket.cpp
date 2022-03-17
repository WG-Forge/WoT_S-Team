#include "socket.h"
#pragma warning(disable: 4996)
#pragma warning(disable: 4200)

namespace web
{
	namespace io_base
	{
		socket::socket() :_socket(), _inited(false)
		{
			ZeroMemory(&_socket_address, sizeof(_socket_address));
		}

		bool socket::init(byte s_b1, byte s_b2, byte s_b3, byte s_b4, ushort port)
		{
			byte addres[4]{ s_b1,s_b2, s_b3, s_b4 };
			return init(*((unsigned long*)&addres), port);
		}

		bool socket::init(uint addres, ushort port)
		{
			sockaddr_in serv_adr;
			serv_adr.sin_family = AF_INET;
			serv_adr.sin_addr.s_addr = addres;
			serv_adr.sin_port = htons(port);
			return init(serv_adr);
		}

		bool socket::init(const char* addres, ushort port)
		{
			return init(inet_addr(addres), port);
		}

		bool socket::init(const sockaddr_in& addr)
		{
			if (_inited) return _inited;
			auto socket = WSASocket(AF_INET, SOCK_STREAM, IPPROTO_TCP, NULL, 0, WSA_FLAG_OVERLAPPED);
			if (INVALID_SOCKET == socket)
			{
				printf("error socket::init WSASocket()\n");
				return false;
			}
			_socket = socket;
			_socket_address = addr;
			_inited = true;
			return _inited;
		}

		bool socket::bind()
		{
			if (::bind(_socket, reinterpret_cast<SOCKADDR*>(&_socket_address), sizeof(_socket_address)) != 0)
			{
				printf("error socket::bind bind()\n");
				return false;
			}
			return true;
		}
		bool socket::bind_before_connect()
		{
			struct sockaddr_in addr;
			memset(&addr, 0, sizeof(addr));

			addr.sin_family = AF_INET;
			addr.sin_addr.s_addr = INADDR_ANY;
			addr.sin_port = 0;

			if (::bind(_socket, reinterpret_cast<SOCKADDR*>(&addr), sizeof(addr)) != 0)
			{
				printf("error socket::bind_before_connect bind()\n");
				return false;
			}
			return true;
		}
		bool socket::listen()
		{
			return listen(1);
		}
		bool socket::listen(int backlog)
		{
			if (::listen(_socket, backlog) != 0)
			{
				printf("error socket::listen listen()\n");
				return false;
			}
			return true;
		}
		const SOCKET& socket::get_socket() const
		{
			return _socket;
		}
		const sockaddr_in& socket::get_socket_address() const
		{
			return _socket_address;
		}
	}
}