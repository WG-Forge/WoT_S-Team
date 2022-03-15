#pragma once
#include "../defs.h"
#include "winsock2.h"

namespace web_client
{
	enum class web_actions
	{
		LOGIN = 1,
		LOGOUT = 2,
		MAP = 3,
		GAME_STATE = 4,
		GAME_ACTIONS = 5,
		TURN = 6,
		CHAT = 100,
		MOVE = 101,
		SHOOT = 102,
	};

	class client
	{
	public:
		client();
		~client();
		result connect(const SOCKADDR_IN& addr);
		result detach();
		result send_packet(web_actions action, int size, byte* data, int* out_size, byte* out_data);

	private:
		result _init();
		bool _send_all(void* buf, int len);
		bool _recv_all(void* buf, int len);

		bool _inited;
		bool _connected;
		SOCKET _socket;
		SOCKADDR_IN _addr;
	};
}
