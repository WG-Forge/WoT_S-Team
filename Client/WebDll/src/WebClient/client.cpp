#include "client.h"

namespace web_client
{
	client::client(): _inited(false), _connected(false), _socket(0)
	{
		ZeroMemory(&_addr, sizeof(_addr));
	}
	client::~client()
	{
		WSACleanup();
	}	
	result client::connect(const SOCKADDR_IN& addr)
	{
		result res;
		if ((res = _init()) != result::OKEY)
			return res;

		detach();
		SOCKET socket;
		if (INVALID_SOCKET == (socket = ::socket(AF_INET, SOCK_STREAM, IPPROTO_TCP)))
			return result::ERROR_SOCKET;

		if (::connect(socket, (SOCKADDR*)&addr, sizeof(addr)) != 0)
		{
			closesocket(socket);
			return result::ERROR_CONNECT;
		}
		_socket = socket;
		_addr = addr;
		_connected = true;
		return result::OKEY;
	}
	result client::detach()
	{
		if (_connected)
		{
			closesocket(_socket);
			_socket = 0;
			return result::OKEY;
		}
		return result::OKEY;
	}
	result client::send_packet(web_actions action, int size, byte* data, int* out_size, byte* out_data)
	{
		if (!_connected) return result::CONNECTED_FALSE;
		if (!_send_all(&action, sizeof(action)) || !_send_all(&size, sizeof(size)))
			return result::ERROR_SEND;

		if (size)
			if (!_send_all(data, size))
				return result::ERROR_SEND;

		result res;
		int packet_size;
		if (!_recv_all(&res, sizeof(res)) || !_recv_all(&packet_size, sizeof(packet_size)))
			return result::ERROR_RECV;

		if (packet_size == 0)
		{
			*out_size = packet_size;
			return res;
		}

		if (!_recv_all(out_data, packet_size))
			return result::ERROR_RECV;
		*out_size = packet_size;
		return res;
	}

	result client::_init()
	{
		if (_inited) return result::OKEY;

		WSADATA wsaData;
		if (WSAStartup(WINSOCK_VERSION, &wsaData))
			return result::ERROR_WSA_INIT;
		_inited = true;
		return result::OKEY;
	}
	bool client::_send_all(void* buf, int len)
	{
		int total = 0;
		int bytesleft = len;
		int n;
		while (total < len)
		{
			n = send(_socket, reinterpret_cast<char*>(buf) + total, bytesleft, 0);
			if (n == -1 || n == 0)
			{
				detach();
				return false;
			}
			total += n;
			bytesleft -= n;
		}
		return true;
	}
	bool client::_recv_all(void* buf, int len)
	{
		auto n = ::recv(_socket, reinterpret_cast<char*>(buf), len, MSG_WAITALL);
		if (n > 0) return true;
		detach();
		return false;
	}
}