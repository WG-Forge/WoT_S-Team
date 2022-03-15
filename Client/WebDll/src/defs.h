#pragma once

typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;

enum class result : int
{
	OKEY = 0,
	BAD_COMMAND = 1,
	ACCESS_DENIED = 2,
	INAPPROPRIATE_GAME_STATE = 3,
	TIMEOUT = 4,
	INTERNAL_SERVER_ERROR = 500,

	// for client
	ERROR_WSA_INIT,
	ERROR_SOCKET,
	ERROR_CONNECT,
	CONNECTED_FALSE,
	ERROR_SEND,
	ERROR_RECV,
	IVALID_PARAM,
};
