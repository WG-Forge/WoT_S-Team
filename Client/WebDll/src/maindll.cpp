#include "WebClient/client.h"
#include <exception>

extern "C"
{
#pragma region web_client
	/////////////////// WEB_CLIENT /////////////////////////////
	__declspec(dllexport) web_client::client* create_wc()
	{
		web_client::client* web = nullptr;
		try
		{
			web = new web_client::client();
			return web;
		}
		catch (const std::exception&)
		{
			return nullptr;
		}
	}

	__declspec(dllexport) result connect_(web_client::client* web, uint addr, ushort port)
	{
		if (!web) return result::IVALID_PARAM;
		SOCKADDR_IN serv_adr;
		serv_adr.sin_addr.s_addr = addr;
		serv_adr.sin_port = htons(port);
		serv_adr.sin_family = AF_INET;
		return web->connect(serv_adr);
	}

	__declspec(dllexport) result detach(web_client::client* web)
	{
		if (!web) return result::IVALID_PARAM;
		return web->detach();
	}

	__declspec(dllexport) result send_packet(web_client::client* web, web_client::web_actions action, int size, byte* data, int* out_size, byte* out_data)
	{
		if (!web) return result::IVALID_PARAM;
		return web->send_packet(action, size, data, out_size, out_data);
	}

	__declspec(dllexport) result destroy_wc(web_client::client* web)
	{
		if (!web) return result::IVALID_PARAM;
		try
		{
			delete web;
			return result::OKEY;
		}
		catch (const std::exception&)
		{
			return result::IVALID_PARAM;
		}
	}
	/////////////////// END WEB_CLIENT /////////////////////////////
#pragma endregion
}